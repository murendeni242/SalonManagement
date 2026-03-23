using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for StaffSchedule.
/// </summary>
public interface IStaffScheduleRepository
{
    /// <summary>
    /// Returns the full weekly schedule for a staff member —
    /// all days they have configured working hours.
    /// Returns an empty list when no schedule has been set up.
    /// </summary>
    /// <param name="staffId">Primary key of the staff member.</param>
    Task<IEnumerable<StaffSchedule>> GetByStaffIdAsync(int staffId);

    /// <summary>
    /// Returns the schedule row for a specific staff member on a specific day,
    /// or null when the staff member does not work that day.
    /// Used by CreateBookingHandler and UpdateBookingHandler to validate availability.
    /// </summary>
    /// <param name="staffId">Primary key of the staff member.</param>
    /// <param name="day">Day of week to look up.</param>
    Task<StaffSchedule?> GetByStaffIdAndDayAsync(int staffId, DayOfWeek day);

    /// <summary>Saves a new schedule row to the database.</summary>
    Task AddAsync(StaffSchedule schedule);

    /// <summary>Saves changes to an existing schedule row.</summary>
    Task UpdateAsync(StaffSchedule schedule);

    /// <summary>
    /// Hard deletes a schedule row.
    /// Used when a staff member no longer works on a given day.
    /// </summary>
    Task DeleteAsync(StaffSchedule schedule);
}