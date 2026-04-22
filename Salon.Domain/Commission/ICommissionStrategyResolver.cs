using Salon.Domain.Enums;

/// <summary>
/// Resolves the correct ICommissionStrategy for a given CommissionType.
/// Registered in DI — inject this wherever commission calculation is needed.
/// </summary>
public interface ICommissionStrategyResolver
{
    ICommissionStrategy Resolve(CommissionType type);
}