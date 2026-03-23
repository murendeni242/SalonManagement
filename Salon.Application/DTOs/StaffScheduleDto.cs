namespace Salon.Application.DTOs
{
    /// <summary>
    /// Full daily schedule for a staff member returned by the schedule endpoint.
    /// </summary>
    public class StaffScheduleDto
    {
        /// <summary>Staff member the schedule belongs to.</summary>
        public int StaffId { get; set; }

        /// <summary>Full display name for the header.</summary>
        public string StaffName { get; set; } = default!;

        /// <summary>The date this schedule covers.</summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Appointments for this day, ordered by StartTime ascending.
        /// Empty list means the staff member has no bookings for this date.
        /// </summary>
        public List<StaffScheduleItemDto> Appointments { get; set; } = new();

        /// <summary>Total number of appointments for the day.</summary>
        public int TotalAppointments => Appointments.Count;
    }
}
