namespace Salon.Application.DTOs
{
    /// <summary>
    /// A single appointment slot on a staff member's daily schedule.
    /// Returned by GET /api/staff/{id}/schedule?date=yyyy-MM-dd.
    /// </summary>
    public class StaffScheduleItemDto
    {
        /// <summary>Booking primary key.</summary>
        public int BookingId { get; set; }

        /// <summary>ID of the customer for this appointment.</summary>
        public int CustomerId { get; set; }

        /// <summary>ID of the service being performed.</summary>
        public int ServiceId { get; set; }

        /// <summary>Calendar date of the appointment.</summary>
        public DateTime BookingDate { get; set; }

        /// <summary>Time the appointment starts.</summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>Time the appointment ends.</summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>Current booking status: Pending | Confirmed | Completed | Cancelled.</summary>
        public string Status { get; set; } = default!;

        /// <summary>Optional notes on the booking.</summary>
        public string? Notes { get; set; }
    }
}
