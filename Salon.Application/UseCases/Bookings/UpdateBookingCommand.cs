namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Input for updating a Pending booking.
/// Confirmed, Completed and Cancelled bookings are immutable.
/// End time is recalculated from the new service DurationMinutes.
/// Only Owner or Reception roles may submit this.
/// </summary>
public class UpdateBookingCommand
{
    /// <summary>Primary key of the booking to update. Set from the URL route parameter.</summary>
    public int Id { get; set; }

    /// <summary>New staff member assignment.</summary>
    public int StaffId { get; set; }

    /// <summary>New service. Price and duration are re-read from this record.</summary>
    public int ServiceId { get; set; }

    /// <summary>New calendar date. Must not be in the past.</summary>
    public DateTime BookingDate { get; set; }

    /// <summary>New start time.</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>Replacement notes, or null to clear. Maximum 500 characters.</summary>
    public string? Notes { get; set; }
}