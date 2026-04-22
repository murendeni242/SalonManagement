using Salon.Domain.Entities;

/// <summary>
/// Calculates commission as a percentage of the payment amount.
/// Example: 40% of R280 = R112.
/// </summary>
public class PercentageCommissionStrategy : ICommissionStrategy
{
    public decimal Calculate(
        decimal paymentAmount,
        CommissionRule rule,
        int completedServicesThisMonth)
    {
        return Math.Round(paymentAmount * rule.RateOrAmount / 100, 2);
    }
}