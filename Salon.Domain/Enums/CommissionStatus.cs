namespace Salon.Domain.Enums;

/// <summary>
/// Lifecycle of a commission record.
/// Pending → Paid is the normal flow.
/// Reversed is set when a refund reduces the commission to zero.
/// </summary>
public enum CommissionStatus
{
    /// <summary>
    /// Commission calculated but not yet paid to the staff member.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Commission has been paid out to the staff member.
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Commission was reversed due to a full refund.
    /// </summary>
    Reversed = 3
}