using Salon.Domain.Common;
using Salon.Domain.Enums;

namespace Salon.Domain.Entities;

/// <summary>
/// Represents a single payment transaction linked to a salon booking.
///
/// Key design decisions for this salon system:
///
/// 1. A booking can have MORE than one sale record.
///    Reason: customers sometimes pay a deposit upfront, then the balance on the day.
///    Both payments link to the same BookingId.
///
/// 2. Refunds create a NEW Sale record with a negative AmountPaid.
///    The original sale is marked Refunded but never modified.
///    Reason: clean financial paper trail — nothing is ever overwritten.
///
/// 3. Void is for erroneous data entry (wrong amount typed, wrong booking).
///    It does NOT mean money was returned. Owner only.
///
/// 4. Valid payment methods: Cash | Card | EFT | Voucher.
///    Enforced in the domain so unrecognised methods cannot enter the system.
///
/// 5. ProcessedByStaffId records who took the payment — useful for commission
///    reporting and end-of-day accountability.
/// </summary>
public class Sale
{
    // ── Identity ──────────────────────────────────────────────────────

    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    // ── Core fields ───────────────────────────────────────────────────

    /// <summary>
    /// The booking this payment is for.
    /// A booking can have multiple sale records (deposit + balance, split payment).
    /// </summary>
    public int BookingId { get; private set; }

    /// <summary>
    /// Amount of this transaction.
    /// Positive for normal payments. Negative for refund records.
    /// </summary>
    public decimal AmountPaid { get; private set; }

    /// <summary>How the customer paid: Cash | Card | EFT | Voucher.</summary>
    public string PaymentMethod { get; private set; } = default!;

    /// <summary>Current status of this payment record.</summary>
    public SaleStatus Status { get; private set; }

    /// <summary>UTC timestamp when this transaction was recorded.</summary>
    public DateTime PaidAt { get; private set; }

    // ── Optional fields ───────────────────────────────────────────────

    /// <summary>
    /// Staff member who processed this payment.
    /// Used for commission reporting and end-of-day accountability.
    /// </summary>
    public int? ProcessedByStaffId { get; private set; }

    /// <summary>
    /// Optional notes, e.g. "Deposit only — balance due on the day".
    /// Also stores the void reason when a sale is voided.
    /// Maximum 500 characters.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// For refund records only: the ID of the original sale that was refunded.
    /// Null on normal payment records.
    /// </summary>
    public int? OriginalSaleId { get; private set; }

    // ── EF Core ───────────────────────────────────────────────────────

    /// <summary>Required by EF Core. Do not call directly.</summary>
    protected Sale() { }

    // ── Constructor ───────────────────────────────────────────────────

    /// <summary>
    /// Records a new payment against a booking. Status is set to Paid automatically.
    /// </summary>
    /// <param name="bookingId">ID of the booking being paid for. Must be greater than 0.</param>
    /// <param name="amountPaid">Amount received. Must be greater than 0.</param>
    /// <param name="paymentMethod">Cash | Card | EFT | Voucher.</param>
    /// <param name="processedByStaffId">Optional: staff member who took the payment.</param>
    /// <param name="notes">Optional notes. Maximum 500 characters.</param>
    /// <exception cref="DomainException">Thrown when any business rule is violated.</exception>
    public Sale(int bookingId, decimal amountPaid, string paymentMethod,
        int? processedByStaffId = null, string? notes = null)
    {
        if (bookingId <= 0)
            throw new DomainException("Booking is required.");
        if (amountPaid <= 0)
            throw new DomainException("Amount must be greater than zero.");

        ValidatePaymentMethod(paymentMethod);
        ValidateNotes(notes);

        BookingId = bookingId;
        AmountPaid = amountPaid;
        PaymentMethod = paymentMethod;
        ProcessedByStaffId = processedByStaffId;
        Notes = notes;
        Status = SaleStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Private constructor used internally by Refund() to create refund records.
    /// Refund records have a negative AmountPaid and link back to the original sale.
    /// </summary>
    private Sale(int bookingId, decimal negativeRefundAmount, string paymentMethod,
        int originalSaleId, int? processedByStaffId, string? notes)
    {
        BookingId = bookingId;
        AmountPaid = negativeRefundAmount; // negative
        PaymentMethod = paymentMethod;
        OriginalSaleId = originalSaleId;
        ProcessedByStaffId = processedByStaffId;
        Notes = notes;
        Status = SaleStatus.Refunded;
        PaidAt = DateTime.UtcNow;
    }

    // ── State-change methods ──────────────────────────────────────────

    /// <summary>
    /// Issues a refund against this sale.
    ///
    /// This method:
    ///   1. Marks this sale as Refunded.
    ///   2. Returns a NEW Sale record representing the refund transaction.
    ///
    /// The caller must persist BOTH the updated original sale and the returned refund record.
    /// The original payment is never deleted — full financial paper trail preserved.
    /// </summary>
    /// <param name="refundAmount">
    /// Amount to refund. Must be greater than 0 and not exceed the original AmountPaid.
    /// </param>
    /// <param name="processedByStaffId">Optional: staff member processing the refund.</param>
    /// <param name="notes">Optional reason for the refund. Maximum 500 characters.</param>
    /// <returns>A new Sale record representing the refund. Caller must persist this.</returns>
    /// <exception cref="DomainException">
    /// Thrown when this sale is not Paid, amount is invalid, or notes exceed 500 characters.
    /// </exception>
    public Sale Refund(decimal refundAmount, int? processedByStaffId = null, string? notes = null)
    {
        if (Status != SaleStatus.Paid)
            throw new DomainException("Only Paid sales can be refunded.");
        if (refundAmount <= 0)
            throw new DomainException("Refund amount must be greater than zero.");
        if (refundAmount > AmountPaid)
            throw new DomainException(
                $"Refund amount cannot exceed the original payment of {AmountPaid:C}.");

        ValidateNotes(notes);

        // Mark this sale as refunded
        Status = SaleStatus.Refunded;

        // Return a new negative-amount record for the refund transaction
        return new Sale(
            bookingId: BookingId,
            negativeRefundAmount: -refundAmount,
            paymentMethod: PaymentMethod,
            originalSaleId: Id,
            processedByStaffId: processedByStaffId,
            notes: notes ?? $"Refund against Sale #{Id}");
    }

    /// <summary>
    /// Voids this sale for erroneous data-entry (wrong amount, wrong booking).
    /// This is NOT a refund — no money goes back to the customer.
    /// The record is kept with Status = Voided and the reason stored in Notes.
    /// Owner role only.
    /// </summary>
    /// <param name="reason">Reason for voiding. Required so there is always an explanation on record.</param>
    /// <exception cref="DomainException">Thrown when the sale is not Paid or reason is blank.</exception>
    public void Void(string reason)
    {
        if (Status != SaleStatus.Paid)
            throw new DomainException("Only Paid sales can be voided.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A reason is required when voiding a sale.");

        Status = SaleStatus.Voided;
        Notes = reason;
    }

    // ── Private helpers ───────────────────────────────────────────────

    private static readonly HashSet<string> ValidMethods =
        new(StringComparer.OrdinalIgnoreCase) { "Cash", "Card", "EFT", "Voucher" };

    private static void ValidatePaymentMethod(string method)
    {
        if (string.IsNullOrWhiteSpace(method) || !ValidMethods.Contains(method))
            throw new DomainException("Payment method must be one of: Cash, Card, EFT, Voucher.");
    }

    private static void ValidateNotes(string? notes)
    {
        if (notes?.Length > 500)
            throw new DomainException("Notes cannot exceed 500 characters.");
    }
}