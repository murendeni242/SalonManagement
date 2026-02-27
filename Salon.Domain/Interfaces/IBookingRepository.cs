using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Booking aggregate.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Returns all bookings for a specific customer.
    /// Soft-deleted records are excluded by global query filter.
    /// </summary>
    /// <param name="customerId">Customer ID to filter by.</param>
    Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);

    /// <summary>Saves a new booking to the database.</summary>
    Task AddAsync(Booking booking);

    /// <summary>
    /// Returns a booking by primary key including soft-deleted rows, or null if not found.
    /// </summary>
    Task<Booking?> GetByIdAsync(int id);

    /// <summary>
    /// Returns a page of non-deleted bookings ordered by BookingDate descending.
    /// Pagination runs in SQL — no full table load.
    /// </summary>
    /// <param name="skip">Records to skip.</param>
    /// <param name="take">Maximum records to return.</param>
    Task<IEnumerable<Booking>> GetPagedAsync(int skip, int take);

    /// <summary>
    /// Returns true when the staff member has a non-cancelled booking
    /// that overlaps the proposed time window on the given date.
    /// </summary>
    /// <param name="excludeBookingId">
    /// Pass the existing booking ID when updating so it does not conflict with its own slot.
    /// </param>
    Task<bool> ExistsOverlappingBookingAsync(
        int staffId, DateTime date, TimeSpan start, TimeSpan end,
        int? excludeBookingId = null);

    /// <summary>Saves changes to an existing booking.</summary>
    Task UpdateAsync(Booking booking);

    Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime from, DateTime to);
}