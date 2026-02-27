using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IServiceRepository.
/// Extends your original with:
/// - IgnoreQueryFilters() on GetByIdAsync so soft-deleted services are still loadable by ID.
/// - UpdateAsync for saving changes after Update() or SoftDelete() is called on the entity.
/// The global query filter in ServiceConfiguration automatically excludes soft-deleted rows
/// from GetAllAsync without any code change needed there.
/// </summary>
public class ServiceRepository : IServiceRepository
{
    private readonly SalonDbContext _context;

    public ServiceRepository(SalonDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task AddAsync(Service service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    /// <remarks>
    /// IgnoreQueryFilters() so soft-deleted services are still loadable by ID.
    /// Handlers that need to show or inspect a deleted service can still do so.
    /// AsNoTracking() is omitted here because Update/SoftDelete handlers need tracking.
    /// </remarks>
    public async Task<Service?> GetByIdAsync(int id)
        => await _context.Services
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);

    /// <inheritdoc />
    /// <remarks>
    /// Soft-deleted rows excluded automatically by the global query filter
    /// added to ServiceConfiguration. No code change needed here.
    /// </remarks>
    public async Task<IEnumerable<Service>> GetAllAsync()
        => await _context.Services
            .AsNoTracking()
            .ToListAsync();

    /// <inheritdoc />
    public async Task UpdateAsync(Service service)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
    }
}