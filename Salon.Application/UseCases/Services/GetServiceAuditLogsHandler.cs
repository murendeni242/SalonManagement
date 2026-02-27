using Salon.Application.DTOs;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Services;

/// <summary>
/// Returns the full audit history for a service, oldest first.
/// Uses the same shared IAuditLogRepository as bookings — just with entityName = "Service".
/// No new tables or infrastructure needed. Owner role only.
/// </summary>
public class GetServiceAuditLogsHandler
{
    private readonly IAuditLogRepository _auditLog;

    public GetServiceAuditLogsHandler(IAuditLogRepository auditLog)
        => _auditLog = auditLog;

    /// <summary>
    /// Returns all audit entries for the service with the given <paramref name="serviceId"/>,
    /// ordered by ChangedAt ascending.
    /// </summary>
    public async Task<IEnumerable<AuditLogDto>> Handle(int serviceId)
    {
        var logs = await _auditLog.GetByEntityAsync("Service", serviceId);

        return logs.Select(l => new AuditLogDto
        {
            Id = l.Id,
            EntityName = l.EntityName,
            EntityId = l.EntityId,
            Action = l.Action,
            Description = l.Description,
            OldValues = l.OldValues,
            NewValues = l.NewValues,
            ChangedBy = l.ChangedBy,
            ChangedAt = l.ChangedAt
        });
    }
}