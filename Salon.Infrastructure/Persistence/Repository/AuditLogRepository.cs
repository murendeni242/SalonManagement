using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IAuditLogRepository.
/// Handles the single shared AuditLog table for the entire system.
/// Register once in DI — works for Bookings, Services, Staff, Sales, etc.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly SalonDbContext _context;

    public AuditLogRepository(SalonDbContext context) => _context = context;

    /// <inheritdoc />
    public async Task AddAsync(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, int entityId)
        => await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderBy(a => a.ChangedAt)
            .ToListAsync();
}