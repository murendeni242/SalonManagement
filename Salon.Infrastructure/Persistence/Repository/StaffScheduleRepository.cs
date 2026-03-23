using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IStaffScheduleRepository.
/// Hard delete only — no soft delete, no global query filter.
/// </summary>
public class StaffScheduleRepository : IStaffScheduleRepository
{
    private readonly SalonDbContext _context;

    public StaffScheduleRepository(SalonDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<IEnumerable<StaffSchedule>> GetByStaffIdAsync(int staffId)
        => await _context.StaffSchedules
            .AsNoTracking()
            .Where(s => s.StaffId == staffId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync();

    /// <inheritdoc />
    public async Task<StaffSchedule?> GetByStaffIdAndDayAsync(int staffId, DayOfWeek day)
        => await _context.StaffSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StaffId == staffId && s.DayOfWeek == day);

    /// <inheritdoc />
    public async Task AddAsync(StaffSchedule schedule)
    {
        _context.StaffSchedules.Add(schedule);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(StaffSchedule schedule)
    {
        _context.StaffSchedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    /// <remarks>Hard delete — row is permanently removed from the database.</remarks>
    public async Task DeleteAsync(StaffSchedule schedule)
    {
        _context.StaffSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
    }
}