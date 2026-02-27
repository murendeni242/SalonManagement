namespace Salon.Domain.Enums;

/// <summary>
/// Lifecycle states a booking can move through.
/// Valid transitions:
///   Pending → Confirmed → Completed
///   Pending → Cancelled
///   Confirmed → Cancelled
/// </summary>
public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4
}