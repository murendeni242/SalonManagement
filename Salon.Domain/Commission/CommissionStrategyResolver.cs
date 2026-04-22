using Salon.Domain.Enums;

/// <summary>
/// Registered in DI — inject this wherever commission calculation is needed.
/// </summary>
public class CommissionStrategyResolver : ICommissionStrategyResolver
{
    public ICommissionStrategy Resolve(CommissionType type) => type switch
    {
        CommissionType.Percentage => new PercentageCommissionStrategy(),
        CommissionType.Fixed => new FixedCommissionStrategy(),
        CommissionType.Tiered => new TieredCommissionStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(type),
                 $"No strategy registered for commission type: {type}")
    };
}