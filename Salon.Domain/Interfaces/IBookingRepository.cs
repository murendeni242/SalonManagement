using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Booking aggregate.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Retrieves all bookings for a specific customer.
    /// Soft-deleted records are excluded by the global query filter.
    /// </summary>
    /// <param name="customerId">The ID of the customer.</param>
    /// <returns>A collection of bookings for the specified customer.</returns>
    Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Saves a new booking to the database.
    /// </summary>
    /// <param name="booking">The booking entity to add.</param>
    Task AddAsync(Booking booking);

    /// <summary>
    /// Retrieves a booking by its primary key, including soft-deleted rows.
    /// Returns null if the booking is not found.
    /// </summary>
    /// <param name="id">The booking ID.</param>
    /// <returns>The booking entity or null.</returns>
    Task<Booking?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a paginated list of non-deleted bookings ordered by BookingDate descending.
    /// Pagination is performed in SQL to avoid loading the full table into memory.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The maximum number of records to return.</param>
    /// <returns>A collection of bookings in the specified page.</returns>
    Task<IEnumerable<Booking>> GetPagedAsync(int skip, int take);

    /// <summary>
    /// Determines whether a staff member has a non-cancelled booking
    /// that overlaps the proposed time window on the given date.
    /// </summary>
    /// <param name="staffId">The ID of the staff member.</param>
    /// <param name="date">The booking date to check.</param>
    /// <param name="start">The proposed start time.</param>
    /// <param name="end">The proposed end time.</param>
    /// <param name="excludeBookingId">
    /// Optional booking ID to exclude from the check (useful when updating an existing booking).
    /// </param>
    /// <returns>True if an overlapping booking exists; otherwise false.</returns>
    Task<bool> ExistsOverlappingBookingAsync(
        int staffId, DateTime date, TimeSpan start, TimeSpan end,
        int? excludeBookingId = null);

    /// <summary>
    /// Updates an existing booking in the database.
    /// </summary>
    /// <param name="booking">The booking entity to update.</param>
    Task UpdateAsync(Booking booking);

    /// <summary>
    /// Deletes a booking from the database (soft delete if configured).
    /// </summary>
    /// <param name="booking">The booking entity to delete.</param>
    Task DeleteAsync(Booking booking);

    /// <summary>
    /// Retrieves all bookings within the specified date range, inclusive.
    /// </summary>
    /// <param name="from">The start date of the range.</param>
    /// <param name="to">The end date of the range.</param>
    /// <returns>A collection of bookings within the specified date range.</returns>
    Task<IEnumerable<Booking>> GetByDateRangeAsync(DateTime from, DateTime to);
}
