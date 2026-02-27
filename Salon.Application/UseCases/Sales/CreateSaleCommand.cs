namespace Salon.Application.UseCases.Sales;

/// <summary>
/// Input for recording a new payment against a booking.
/// Owner or Reception role may submit this.
/// </summary>
public class CreateSaleCommand
{
    /// <summary>ID of the booking being paid for.</summary>
    public int BookingId { get; set; }

    /// <summary>Amount received. Must be greater than 0.</summary>
    public decimal AmountPaid { get; set; }

    /// <summary>How the customer paid: Cash | Card | EFT | Voucher.</summary>
    public string PaymentMethod { get; set; } = default!;

    /// <summary>Optional: ID of the staff member who processed this payment.</summary>
    public int? ProcessedByStaffId { get; set; }

    /// <summary>Optional notes, e.g. "Deposit only". Maximum 500 characters.</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Input for issuing a refund against an existing Paid sale.
/// Owner role only.
/// </summary>
public class RefundSaleCommand
{
    /// <summary>Primary key of the sale to refund. Set from the URL route parameter.</summary>
    public int SaleId { get; set; }

    /// <summary>
    /// Amount to refund. Must be greater than 0 and not exceed the original AmountPaid.
    /// Partial refunds are allowed.
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>Optional: staff member processing the refund.</summary>
    public int? ProcessedByStaffId { get; set; }

    /// <summary>Optional reason for the refund. Maximum 500 characters.</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Input for voiding an erroneous sale entry.
/// Use this when a payment was recorded with the wrong amount or wrong booking.
/// This is NOT a refund — no money goes back to the customer.
/// Owner role only.
/// </summary>
public class VoidSaleCommand
{
    /// <summary>Primary key of the sale to void. Set from the URL route parameter.</summary>
    public int SaleId { get; set; }

    /// <summary>
    /// Reason for voiding. Required so there is always an explanation on record.
    /// </summary>
    public string Reason { get; set; } = default!;
}