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

    /// <summary>
    /// Retrieves all bookings for the specified customer ordered by most recent date and time.
    /// </summary>
    public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.CustomerId == customerId)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new booking to the database.
    /// </summary>
    public async Task AddAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a booking by its unique identifier.
    /// </summary>
    /// <inheritdoc />
    /// <remarks>
    /// IgnoreQueryFilters() lets handlers see soft-deleted bookings by ID.
    /// AsNoTracking() keeps the query read-optimised.
    /// </remarks>
    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <summary>
    /// Retrieves a paginated list of bookings ordered by most recent booking date.
    /// </summary>
    /// <inheritdoc />
    /// <remarks>
    /// Soft-deleted rows are excluded automatically by the global query filter.
    /// Skip and Take are executed in SQL to avoid in-memory pagination.
    /// </remarks>
    public async Task<IEnumerable<Booking>> GetPagedAsync(int skip, int take)
    {
        return await _context.Bookings
            .AsNoTracking()
            .OrderByDescending(b => b.BookingDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }


    /// <inheritdoc />
    /// <summary>
    /// Checks whether a booking exists that overlaps with the specified
    /// time range for a staff member on a given date.
    /// Cancelled bookings are ignored. The optional excludeBookingId
    /// allows updates to ignore the booking currently being modified.
    /// </summary>
    public async Task<bool> ExistsOverlappingBookingAsync(
        int staffId, DateTime date, TimeSpan start, TimeSpan end,
        int? excludeBookingId = null)
    {
        return await _context.Bookings.AnyAsync(b =>
            b.StaffId == staffId &&
            b.BookingDate == date &&
            b.Status != BookingStatus.Cancelled &&
            (excludeBookingId == null || b.Id != excludeBookingId) &&
            start < b.EndTime &&
            end > b.StartTime
        );
    }

    /// <summary>
    /// Updates an existing booking in the database.
    /// </summary>
    /// <inheritdoc />
    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a booking from the database.
    /// </summary>
    /// <inheritdoc />
    public async Task DeleteAsync(Booking booking)
    {
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all bookings within the specified date range, inclusive.
    /// </summary>
    /// <inheritdoc />
    public async Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.Bookings
            .Where(b => b.BookingDate >= from && b.BookingDate <= to)
            .ToListAsync();
    }

}