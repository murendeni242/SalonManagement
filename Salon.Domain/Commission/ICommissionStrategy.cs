using Salon.Domain.Entities;

/// <summary>
/// Contract for all commission calculation strategies.
/// Each strategy takes the payment amount, the rule, and (for tiered)
/// the number of completed services this month.
/// </summary>
public interface ICommissionStrategy
{
    /// <summary>
    /// Calculates the commission amount for a single payment.
    /// </summary>
    /// <param name="paymentAmount">The gross payment amount from the Sale.</param>
    /// <param name="rule">The commission rule configured for this staff member.</param>
    /// <param name="completedServicesThisMonth">
    /// Number of completed bookings for this staff member in the current calendar month.
    /// Only used by TieredCommissionStrategy — ignored by others.
    /// </param>
    /// <returns>Commission amount rounded to 2 decimal places.</returns>
    decimal Calculate(decimal paymentAmount, CommissionRule rule, int completedServicesThisMonth);
}