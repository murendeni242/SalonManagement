using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for the shared AuditLog table.
/// One interface covers auditing for the entire system.
/// Register once in DI, inject into any handler that needs auditing.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Appends a new audit entry. Never updates or deletes existing entries.
    /// </summary>
    Task AddAsync(AuditLog log);

    /// <summary>
    /// Returns all audit entries for a specific entity, ordered by ChangedAt ascending
    /// so callers see the full lifecycle from creation to the latest change.
    /// </summary>
    /// <param name="entityName">Entity type, e.g. "Booking".</param>
    /// <param name="entityId">Primary key of the entity.</param>
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, int entityId);
}