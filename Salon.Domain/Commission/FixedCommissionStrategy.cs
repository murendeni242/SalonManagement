using Salon.Domain.Entities;

/// <summary>
/// Calculates commission as a flat amount per payment regardless of value.
/// Example: R50 per service completed.
/// Note: If the payment amount is less than the fixed rate, commission
/// is capped at the payment amount — you cannot earn more than was paid.
/// </summary>
public class FixedCommissionStrategy : ICommissionStrategy
{
    public decimal Calculate(
        decimal paymentAmount,
        CommissionRule rule,
        int completedServicesThisMonth)
    {
        return Math.Min(rule.RateOrAmount, paymentAmount);
    }
}