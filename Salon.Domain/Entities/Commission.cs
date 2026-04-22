using Salon.Domain.Common;
using Salon.Domain.Enums;

namespace Salon.Domain.Entities;

/// <summary>
/// Records the commission earned by a staff member for a single payment (Sale).
///
/// Design decisions:
///   - One Commission per Sale — because commission is calculated per payment.
///   - Stores RateApplied and Type as a snapshot — so historical records
///     remain accurate even if the rule changes later.
///   - Pending → Paid lifecycle with PaidAt timestamp.
///   - Reversed when a full refund brings the amount to zero.
/// </summary>
public class Commission
{
    // ── Identity ──────────────────────────────────────────────────

    public int Id { get; private set; }

    // ── Foreign keys ──────────────────────────────────────────────

    /// <summary>The payment this commission was calculated against.</summary>
    public int SaleId { get; private set; }

    /// <summary>The staff member who earned this commission.</summary>
    public int StaffId { get; private set; }

    // ── Commission details ────────────────────────────────────────

    /// <summary>Gross commission amount before any refund adjustment.</summary>
    public decimal GrossAmount { get; private set; }

    /// <summary>
    /// Current commission amount after any refund adjustments.
    /// This is what the staff member actually earns.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// The rate or amount from the rule at the time of calculation.
    /// Snapshotted so historical records stay accurate if the rule changes.
    /// </summary>
    public decimal RateApplied { get; private set; }

    /// <summary>Strategy type used — snapshotted for the same reason as RateApplied.</summary>
    public CommissionType Type { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────

    public CommissionStatus Status { get; private set; } = CommissionStatus.Pending;

    /// <summary>UTC timestamp when the commission was marked as paid.</summary>
    public DateTime? PaidAt { get; private set; }

    /// <summary>Who marked this commission as paid.</summary>
    public string? PaidBy { get; private set; }

    // ── Audit ─────────────────────────────────────────────────────

    public DateTime CreatedAt { get; private set; }

    // ── EF Core ───────────────────────────────────────────────────

    protected Commission() { }

    // ── Constructor ───────────────────────────────────────────────

    /// <summary>
    /// Creates a new pending commission record for a payment.
    /// </summary>
    /// <param name="saleId">FK to the Sale this commission belongs to.</param>
    /// <param name="staffId">FK to the staff member who earned this.</param>
    /// <param name="amount">Calculated commission amount.</param>
    /// <param name="rateApplied">Rate or amount from the rule — snapshotted.</param>
    /// <param name="type">Strategy type — snapshotted.</param>
    public Commission(int saleId, int staffId, decimal amount,
        decimal rateApplied, CommissionType type)
    {
        if (amount < 0)
            throw new DomainException("Commission amount cannot be negative.");

        SaleId = saleId;
        StaffId = staffId;
        GrossAmount = amount;
        Amount = amount;
        RateApplied = rateApplied;
        Type = type;
        Status = CommissionStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // ── State-change methods ──────────────────────────────────────

    /// <summary>
    /// Adjusts the commission amount after a refund.
    /// Calculated proportionally: newAmount = grossAmount * (refundedPayment / originalPayment).
    /// Sets status to Reversed when amount reaches zero.
    /// </summary>
    /// <param name="newAmount">The recalculated commission amount after refund.</param>
    /// <exception cref="DomainException">Thrown when called on a Paid commission.</exception>
    public void AdjustForRefund(decimal newAmount)
    {
        if (Status == CommissionStatus.Paid)
            throw new DomainException(
                "Cannot adjust a commission that has already been paid out.");

        if (newAmount < 0)
            throw new DomainException("Adjusted commission amount cannot be negative.");

        Amount = newAmount;
        Status = newAmount == 0
            ? CommissionStatus.Reversed
            : CommissionStatus.Pending;
    }

    /// <summary>
    /// Marks this commission as paid to the staff member.
    /// </summary>
    /// <param name="paidBy">Email of the user marking the commission as paid.</param>
    /// <exception cref="DomainException">Thrown when already paid or reversed.</exception>
    public void MarkPaid(string paidBy)
    {
        if (Status == CommissionStatus.Paid)
            throw new DomainException("Commission has already been paid.");

        if (Status == CommissionStatus.Reversed)
            throw new DomainException("Cannot mark a reversed commission as paid.");

        Status = CommissionStatus.Paid;
        PaidAt = DateTime.UtcNow;
        PaidBy = paidBy;
    }
}