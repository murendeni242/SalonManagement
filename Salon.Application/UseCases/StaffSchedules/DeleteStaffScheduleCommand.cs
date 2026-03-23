namespace Salon.Application.UseCases.StaffSchedules
{
    /// <summary>
    /// Command used to remove a staff member's schedule for a specific day.
    /// Typically used when a staff member is not available on that day.
    /// </summary>
    public class DeleteStaffScheduleCommand
    {
        /// <summary>
        /// The ID of the staff member whose schedule should be removed.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// The day of the week for which the schedule will be deleted.
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }
    }
}
