using Salon.Domain.Entities;

/// <summary>
/// Calculates commission based on which tier the staff member is currently in for the current calendar month.
/// </summary>
public class TieredCommissionStrategy : ICommissionStrategy
{
    public decimal Calculate(
        decimal paymentAmount,
        CommissionRule rule,
        int completedServicesThisMonth)
    {
        if (!rule.Tiers.Any()) return 0;

        var matchingTier = rule.Tiers
            .OrderByDescending(t => t.MinServices)
            .FirstOrDefault(t => t.Matches(completedServicesThisMonth));

        if (matchingTier is null) return 0;

        return Math.Round(paymentAmount * matchingTier.Percentage / 100, 2);
    }
}