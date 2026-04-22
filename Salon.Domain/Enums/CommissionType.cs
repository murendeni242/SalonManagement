namespace Salon.Domain.Enums;

/// <summary>
/// Defines the three supported commission calculation strategies.
/// </summary>
public enum CommissionType
{
    /// <summary>
    /// Staff earns a fixed percentage of the payment amount.
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// Staff earns a flat fixed amount per payment regardless of value.
    /// </summary>
    Fixed = 2,

    /// <summary>
    /// Percentage increases based on number of completed services in the current calendar month.
    /// </summary>
    Tiered = 3
}