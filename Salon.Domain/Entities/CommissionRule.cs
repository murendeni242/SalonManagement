using Salon.Domain.Common;
using Salon.Domain.Enums;

namespace Salon.Domain.Entities;

/// <summary>
/// Defines how commission is calculated for a specific staff member.
/// One rule per staff member — supports Percentage, Fixed, and Tiered strategies.
///
/// Design decisions:
///   - Staff-level rules only for now. Service-specific rules can be layered
///     on top later using a "most specific wins" resolver without breaking this model.
///   - For Percentage: RateOrAmount holds the percentage (e.g. 40 = 40%).
///   - For Fixed: RateOrAmount holds the flat amount (e.g. 50 = R50 per service).
///   - For Tiered: RateOrAmount is unused — tiers are stored in CommissionTiers.
///   - Hard delete — commission rules are configuration data, no audit trail needed.
/// </summary>
public class CommissionRule
{
    // ── Identity ──────────────────────────────────────────────────

    public int Id { get; private set; }

    // ── Foreign key ───────────────────────────────────────────────

    /// <summary>The staff member this rule applies to.</summary>
    public int StaffId { get; private set; }

    public Staff Staff { get; private set; } = default!;

    // ── Rule configuration ────────────────────────────────────────

    /// <summary>Strategy used to calculate commission for this staff member.</summary>
    public CommissionType Type { get; private set; }

    /// <summary>
    /// For Percentage: the percentage value (e.g. 40 means 40%).
    /// For Fixed: the flat amount per payment (e.g. 50 means R50).
    /// For Tiered: not used — see CommissionTiers navigation property.
    /// </summary>
    public decimal RateOrAmount { get; private set; }

    // ── Navigation ────────────────────────────────────────────────

    /// <summary>
    /// Tier definitions when Type == Tiered.
    /// Ordered by MinServices ascending.
    /// Empty for Percentage and Fixed rules.
    /// </summary>
    public ICollection<CommissionTier> Tiers { get; private set; } = new List<CommissionTier>();

    // ── Audit ─────────────────────────────────────────────────────

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // ── EF Core ───────────────────────────────────────────────────

    protected CommissionRule() { }

    // ── Constructor ───────────────────────────────────────────────

    /// <summary>Creates a Percentage or Fixed commission rule.</summary>
    /// <param name="staffId">FK to the staff member.</param>
    /// <param name="type">Percentage or Fixed only — use CreateTiered for tiered rules.</param>
    /// <param name="rateOrAmount">Percentage value or flat amount.</param>
    /// <exception cref="DomainException">Thrown when rateOrAmount is negative or type is Tiered.</exception>
    public CommissionRule(int staffId, CommissionType type, decimal rateOrAmount)
    {
        if (type == CommissionType.Tiered)
            throw new DomainException(
                "Use the Tiered constructor to create a tiered commission rule.");

        if (rateOrAmount < 0)
            throw new DomainException("Commission rate or amount cannot be negative.");

        if (type == CommissionType.Percentage && rateOrAmount > 100)
            throw new DomainException("Percentage cannot exceed 100.");

        StaffId = staffId;
        Type = type;
        RateOrAmount = rateOrAmount;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Creates a Tiered commission rule. Tiers are added separately.</summary>
    public CommissionRule(int staffId)
    {
        StaffId = staffId;
        Type = CommissionType.Tiered;
        RateOrAmount = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── State-change methods ──────────────────────────────────────

    /// <summary>Updates the rate or amount for Percentage and Fixed rules.</summary>
    /// <exception cref="DomainException">Thrown when called on a Tiered rule.</exception>
    public void UpdateRate(decimal rateOrAmount)
    {
        if (Type == CommissionType.Tiered)
            throw new DomainException(
                "Cannot set a flat rate on a tiered rule. Update the tiers instead.");

        if (rateOrAmount < 0)
            throw new DomainException("Commission rate or amount cannot be negative.");

        if (Type == CommissionType.Percentage && rateOrAmount > 100)
            throw new DomainException("Percentage cannot exceed 100.");

        RateOrAmount = rateOrAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Changes the commission strategy type.</summary>
    public void ChangeType(CommissionType type, decimal rateOrAmount = 0)
    {
        Type = type;
        RateOrAmount = rateOrAmount;
        UpdatedAt = DateTime.UtcNow;
    }
}