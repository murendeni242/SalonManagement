namespace Salon.Application.UseCases.StaffSchedules
{
    /// <summary>
    /// Command used to create or update (upsert) a staff member's working schedule for a specific day.
    /// If a schedule already exists for that staff member and day, it will be updated.
    /// Otherwise, a new schedule will be created.
    /// </summary>
    public class UpsertStaffScheduleCommand
    {
        /// <summary>
        /// The ID of the staff member this schedule belongs to.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// The day of the week this schedule applies to (e.g. Monday, Tuesday).
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// The time the staff member starts working on this day.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// The time the staff member finishes working on this day.
        /// </summary>
        public TimeSpan EndTime { get; set; }
    }
}
