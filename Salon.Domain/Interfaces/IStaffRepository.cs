using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Staff entity.
/// Your existing GetAllAsync and GetByIdAsync are kept — three new methods added.
/// </summary>
public interface IStaffRepository
{
    /// <summary>Saves a new staff record to the database.</summary>
    Task AddAsync(Staff staff);

    /// <summary>
    /// Returns all non-deleted staff members.
    /// Soft-deleted records excluded automatically by the EF Core global query filter.
    /// </summary>
    Task<IEnumerable<Staff>> GetAllAsync();

    /// <summary>
    /// Returns a staff member by primary key including soft-deleted records, or null if not found.
    /// Uses IgnoreQueryFilters() so the handler can still load a deleted profile when needed.
    /// </summary>
    Task<Staff?> GetByIdAsync(int id);

    /// <summary>
    /// Returns a staff member by their email address, or null if not found.
    /// Used to link a Staff profile to a User login account on the schedule endpoint.
    /// </summary>
    Task<Staff?> GetByEmailAsync(string email);              // ✅ NEW

    /// <summary>
    /// Returns all non-cancelled bookings assigned to a staff member on a specific date,
    /// ordered by StartTime ascending.
    /// Used to render the daily schedule / calendar view.
    /// </summary>
    /// <param name="staffId">Primary key of the staff member.</param>
    /// <param name="date">The calendar date to fetch the schedule for.</param>
    Task<IEnumerable<Booking>> GetScheduleAsync(int staffId, DateTime date); // ✅ NEW

    /// <summary>Saves changes to an existing staff record.</summary>
    Task UpdateAsync(Staff staff);                           // ✅ NEW
}