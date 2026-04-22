using Salon.Domain.Common;

namespace Salon.Domain.Entities;

/// <summary>
/// A single tier in a tiered commission rule.
/// Example: 0–10 services = 30%, 11–30 = 40%, 30+ = 50%.
///
/// MaxServices of null means "unlimited" — used for the top tier.
/// </summary>
public class CommissionTier
{
    public int Id { get; private set; }

    /// <summary>FK to the parent CommissionRule.</summary>
    public int CommissionRuleId { get; private set; }

    public CommissionRule CommissionRule { get; private set; } = default!;

    /// <summary>Minimum number of completed services this month to qualify.</summary>
    public int MinServices { get; private set; }

    /// <summary>
    /// Maximum number of completed services this month for this tier.
    /// Null means no upper limit — this is the top tier.
    /// </summary>
    public int? MaxServices { get; private set; }

    /// <summary>Commission percentage for this tier (e.g. 40 = 40%).</summary>
    public decimal Percentage { get; private set; }

    // ── EF Core ───────────────────────────────────────────────────

    protected CommissionTier() { }

    // ── Constructor ───────────────────────────────────────────────

    /// <summary>Creates a commission tier.</summary>
    /// <param name="commissionRuleId">FK to the parent rule.</param>
    /// <param name="minServices">Minimum services to qualify for this tier.</param>
    /// <param name="maxServices">Maximum services — null for the top tier.</param>
    /// <param name="percentage">Percentage to apply (0–100).</param>
    /// <exception cref="DomainException">Thrown when percentage is invalid or min > max.</exception>
    public CommissionTier(int commissionRuleId, int minServices, int? maxServices, decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new DomainException("Tier percentage must be between 0 and 100.");

        if (maxServices.HasValue && maxServices < minServices)
            throw new DomainException("MaxServices must be greater than MinServices.");

        CommissionRuleId = commissionRuleId;
        MinServices = minServices;
        MaxServices = maxServices;
        Percentage = percentage;
    }

    /// <summary>Returns true when the given service count falls in this tier.</summary>
    public bool Matches(int completedServices)
    {
        if (completedServices < MinServices) return false;
        if (MaxServices.HasValue && completedServices > MaxServices) return false;
        return true;
    }
}