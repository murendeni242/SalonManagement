namespace Salon.Application.DTOs
{
    /// <summary>
    /// Working hours configuration for a staff member on one day of the week.
    /// </summary>
    public class WorkingHoursDto
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>Display label — e.g. "Monday".</summary>
        public string DayName { get; set; } = default!;

        /// <summary>Start time as string — e.g. "09:00".</summary>
        public string StartTime { get; set; } = default!;

        /// <summary>End time as string — e.g. "17:00".</summary>
        public string EndTime { get; set; } = default!;
    }
}
