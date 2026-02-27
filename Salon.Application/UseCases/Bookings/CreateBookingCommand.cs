namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Input for creating a new booking.
/// End time is derived automatically from the service DurationMinutes.
/// Only Owner or Reception roles may submit this.
/// </summary>
public class CreateBookingCommand
{
    /// <summary>ID of the customer making the appointment.</summary>
    public int CustomerId { get; set; }

    /// <summary>ID of the staff member who will perform the service.</summary>
    public int StaffId { get; set; }

    /// <summary>ID of the service. Duration and price are read from this record.</summary>
    public int ServiceId { get; set; }

    /// <summary>Calendar date for the appointment. Must not be in the past.</summary>
    public DateTime BookingDate { get; set; }

    /// <summary>Time of day the appointment begins.</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>Optional notes. Maximum 500 characters.</summary>
    public string? Notes { get; set; }
}