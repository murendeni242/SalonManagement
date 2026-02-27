using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Service entity.
/// Your existing methods are unchanged. Three new methods added below.
/// </summary>
public interface IServiceRepository
{
    /// <summary>Saves a new service to the database.</summary>
    Task AddAsync(Service service);

    /// <summary>
    /// Returns a service by primary key including soft-deleted ones, or null if not found.
    /// </summary>
    Task<Service?> GetByIdAsync(int id);

    /// <summary>
    /// Returns all non-deleted services.
    /// Soft-deleted services are excluded automatically by the global query filter.
    /// </summary>
    Task<IEnumerable<Service>> GetAllAsync();

    /// <summary>Saves changes to an existing service.</summary>
    Task UpdateAsync(Service service);  // ✅ NEW
}