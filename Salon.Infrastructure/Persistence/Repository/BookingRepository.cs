using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IBookingRepository.
///
/// Soft-delete note: BookingConfiguration adds a global query filter
/// (WHERE IsDeleted = 0) so every standard LINQ query automatically
/// excludes soft-deleted rows. GetByIdAsync uses IgnoreQueryFilters()
/// so handlers can still load deleted bookings when needed.
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly SalonDbContext _context;

    public BookingRepository(SalonDbContext context) => _context = context;

    public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
    => await _context.Bookings
        .AsNoTracking()
        .Where(b => b.CustomerId == customerId)
        .OrderByDescending(b => b.BookingDate)
        .ThenByDescending(b => b.StartTime)
        .ToListAsync();

    /// <inheritdoc />
    public async Task AddAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    /// <remarks>
    /// IgnoreQueryFilters() lets handlers see soft-deleted bookings by ID.
    /// AsNoTracking() keeps the query read-optimised.
    /// </remarks>
    public async Task<Booking?> GetByIdAsync(int id)
        => await _context.Bookings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

    /// <inheritdoc />
    /// <remarks>
    /// Soft-deleted rows excluded automatically by the global query filter.
    /// Skip/Take pushed into SQL — no in-memory pagination.
    /// </remarks>
    public async Task<IEnumerable<Booking>> GetPagedAsync(int skip, int take)
        => await _context.Bookings
            .AsNoTracking()
            .OrderByDescending(b => b.BookingDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

    /// <inheritdoc />
    /// <remarks>
    /// Three overlap conditions cover all edge cases:
    ///   1. New start falls inside an existing slot.
    ///   2. New end falls inside an existing slot.
    ///   3. New window completely wraps an existing slot.
    /// Cancelled bookings are excluded. excludeBookingId lets an update
    /// avoid conflicting with its own current slot.
    /// </remarks>
    public async Task<bool> ExistsOverlappingBookingAsync(
        int staffId, DateTime date, TimeSpan start, TimeSpan end,
        int? excludeBookingId = null)
        => await _context.Bookings.AnyAsync(b =>
            b.StaffId == staffId &&
            b.BookingDate == date &&
            b.Status != BookingStatus.Cancelled &&
            (excludeBookingId == null || b.Id != excludeBookingId) &&
            (
                (start >= b.StartTime && start < b.EndTime) ||
                (end > b.StartTime && end <= b.EndTime) ||
                (start <= b.StartTime && end >= b.EndTime)
            ));

    /// <inheritdoc />
    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime from, DateTime to)
    => await _context.Bookings
        .Where(b => b.BookingDate >= from && b.BookingDate <= to)
        .ToListAsync();
}