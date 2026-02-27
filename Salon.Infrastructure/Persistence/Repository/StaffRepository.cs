using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IStaffRepository.
///
/// Extends your original with:
/// - IgnoreQueryFilters() on GetByIdAsync so soft-deleted records are still loadable.
/// - GetByEmailAsync for linking a Staff profile to a User login account.
/// - GetScheduleAsync for the daily schedule / calendar view.
/// - UpdateAsync for saving changes after Update(), SetStatus(), or SoftDelete().
///
/// The global query filter in StaffConfiguration automatically excludes soft-deleted
/// records from GetAllAsync without any extra code.
/// </summary>
public class StaffRepository : IStaffRepository
{
    private readonly SalonDbContext _context;

    public StaffRepository(SalonDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task AddAsync(Staff staff)
    {
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Global query filter excludes soft-deleted records automatically.
    /// </remarks>
    public async Task<IEnumerable<Staff>> GetAllAsync()
        => await _context.Staff
            .AsNoTracking()
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();

    /// <inheritdoc />
    /// <remarks>
    /// IgnoreQueryFilters() so the handler can load a deleted record by ID.
    /// Tracking ON (no AsNoTracking) so Update() and SoftDelete() changes are tracked.
    /// </remarks>
    public async Task<Staff?> GetByIdAsync(int id)
        => await _context.Staff
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);

    /// <inheritdoc />
    public async Task<Staff?> GetByEmailAsync(string email)
        => await _context.Staff
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email == email.ToLowerInvariant());

    /// <inheritdoc />
    /// <remarks>
    /// Returns non-cancelled bookings only — cancelled appointments don't occupy the slot.
    /// Ordered by StartTime so the schedule reads chronologically.
    /// BookingDate comparison uses .Date to ignore the time component.
    /// </remarks>
    public async Task<IEnumerable<Booking>> GetScheduleAsync(int staffId, DateTime date)
        => await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.StaffId == staffId &&
                b.BookingDate == date.Date &&
                b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.StartTime)
            .ToListAsync();

    /// <inheritdoc />
    public async Task UpdateAsync(Staff staff)
    {
        _context.Staff.Update(staff);
        await _context.SaveChangesAsync();
    }
}