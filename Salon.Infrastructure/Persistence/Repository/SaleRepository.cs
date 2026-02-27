using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISaleRepository.
///
/// Note: Sales have NO soft-delete and NO global query filter.
/// Financial records must be kept permanently — Voided and Refunded
/// statuses are how "deleted" records are identified.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly SalonDbContext _context;

    public SaleRepository(SalonDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task<IEnumerable<Sale>> GetByCustomerIdAsync(int customerId)
    {
        var customerBookingIds = await _context.Bookings
            .Where(b => b.CustomerId == customerId)
            .Select(b => b.Id)
            .ToListAsync();

        return await _context.Sales
            .Where(s => customerBookingIds.Contains(s.BookingId))
            .OrderByDescending(s => s.PaidAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(Sale sale)
    {
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<Sale?> GetByIdAsync(int id)
        => await _context.Sales
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

    /// <inheritdoc />
    /// <remarks>
    /// Returns ALL records for the booking including refund records (negative amounts)
    /// and voided entries. The caller sees the full payment lifecycle.
    /// Ordered by PaidAt ascending so the payment history reads chronologically.
    /// </remarks>
    public async Task<IEnumerable<Sale>> GetByBookingIdAsync(int bookingId)
        => await _context.Sales
            .AsNoTracking()
            .Where(s => s.BookingId == bookingId)
            .OrderBy(s => s.PaidAt)
            .ToListAsync();

    /// <inheritdoc />
    /// <remarks>
    /// Date filter uses PaidAt.Date so the full day is included when the caller
    /// passes midnight boundaries.
    /// Ordered by PaidAt descending — most recent transactions shown first.
    /// </remarks>
    public async Task<IEnumerable<Sale>> GetPagedAsync(
        DateTime? from, DateTime? to, int skip, int take)
    {
        var query = _context.Sales.AsNoTracking().AsQueryable();

        if (from.HasValue)
            query = query.Where(s => s.PaidAt >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(s => s.PaidAt < to.Value.Date.AddDays(1));

        return await query
            .OrderByDescending(s => s.PaidAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Sale sale)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Sale>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var endDate = to.Date.AddDays(1);

        return await _context.Sales
            .AsNoTracking()
            .Where(s => s.PaidAt >= from && s.PaidAt < endDate)
            .ToListAsync();
    }
}