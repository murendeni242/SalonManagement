using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Sale entity.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Returns all sales linked to a specific customer.
    /// Implemented via Booking relationship.
    /// </summary>
    /// <param name="customerId">Customer ID to filter by.</param>
    Task<IEnumerable<Sale>> GetByCustomerIdAsync(int customerId);


    /// <summary>Saves a new sale record to the database.</summary>
    Task AddAsync(Sale sale);

    /// <summary>Returns a sale by primary key, or null if not found.</summary>
    Task<Sale?> GetByIdAsync(int id);

    /// <summary>
    /// Returns all sale records for a specific booking ordered by PaidAt ascending.
    /// Includes refund records (negative AmountPaid) so the caller sees
    /// the full payment history for that booking.
    /// </summary>
    /// <param name="bookingId">Booking to look up payments for.</param>
    Task<IEnumerable<Sale>> GetByBookingIdAsync(int bookingId);

    /// <summary>
    /// Returns a page of sales filtered by optional date range, ordered by PaidAt descending.
    /// Used for daily / weekly / monthly revenue reports on the Owner dashboard.
    /// </summary>
    /// <param name="from">Optional inclusive start date filter.</param>
    /// <param name="to">Optional inclusive end date filter.</param>
    /// <param name="skip">Records to skip (pagination offset).</param>
    /// <param name="take">Maximum records to return (page size).</param>
    Task<IEnumerable<Sale>> GetPagedAsync(DateTime? from, DateTime? to, int skip, int take);

    /// <summary>
    /// Saves changes to an existing sale.
    /// Used after Refund() marks the original as Refunded, or after Void().
    /// </summary>
    Task UpdateAsync(Sale sale);

    Task<List<Sale>> GetByDateRangeAsync(DateTime from, DateTime to);
}