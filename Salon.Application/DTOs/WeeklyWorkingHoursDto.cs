namespace Salon.Application.DTOs
{
    /// <summary>
    /// Full weekly working hours configuration for a staff member.
    /// Returned by GetWeeklyScheduleHandler.
    /// </summary>
    public class WeeklyWorkingHoursDto
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = default!;

        /// <summary>
        /// One entry per configured working day.
        /// Days with no entry mean the staff member does not work that day.
        /// </summary>
        public List<WorkingHoursDto> WorkingDays { get; set; } = new();
    }
}
