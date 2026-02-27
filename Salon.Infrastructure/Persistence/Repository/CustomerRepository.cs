using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ICustomerRepository.
///
/// Notable additions over your original:
/// - GetByIdAsync uses IgnoreQueryFilters() so deleted records are still loadable by ID.
/// - SearchAsync does a fast SQL LIKE query across name and phone — no in-memory filtering.
/// - GetPagedAsync pushes Skip/Take to SQL so large customer lists stay performant.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly SalonDbContext _context;

    public CustomerRepository(SalonDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task AddAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    /// <remarks>
    /// IgnoreQueryFilters() allows loading soft-deleted records by ID.
    /// Tracking ON so Update/SoftDelete changes are tracked correctly.
    /// </remarks>
    public async Task<Customer?> GetByIdAsync(int id)
        => await _context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);

    /// <inheritdoc />
    /// <remarks>
    /// Ordered alphabetically by LastName then FirstName.
    /// Soft-deleted excluded by global query filter.
    /// </remarks>
    public async Task<IEnumerable<Customer>> GetPagedAsync(int skip, int take)
        => await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

    /// <inheritdoc />
    /// <remarks>
    /// Searches across FirstName, LastName, combined FirstName+LastName, and Phone.
    /// EF Core translates Contains() to SQL LIKE '%term%'.
    /// Capped at 20 results to keep the reception picker fast.
    /// Soft-deleted excluded by global query filter.
    /// </remarks>
    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        var term = searchTerm.ToLower().Trim();

        return await _context.Customers
            .AsNoTracking()
            .Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                (c.FirstName.ToLower() + " " + c.LastName.ToLower()).Contains(term) ||
                c.Phone.Contains(term))
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Take(20)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByEmailAsync(string email)
        => await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant());

    /// <inheritdoc />
    public async Task<Customer?> GetByPhoneAsync(string phone)
        => await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Phone == phone.Trim());

    /// <inheritdoc />
    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Customer>> GetAllAsync()
        => await _context.Customers
            .AsNoTracking()
            .ToListAsync();
}