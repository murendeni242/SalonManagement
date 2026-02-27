using Salon.Application.DTOs;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Returns the full audit history for a booking, oldest first.
///
/// Uses the shared IAuditLogRepository with entityName = "Booking".
/// When you add auditing to Services or Staff later, create a GetAuditLogsHandler
/// in those feature folders that calls the same repository with entityName = "Service" etc.
/// No new tables or infrastructure needed.
///
/// Only the Owner role should have access to this endpoint.
/// </summary>
public class GetAuditLogsHandler
{
    private readonly IAuditLogRepository _auditLog;

    public GetAuditLogsHandler(IAuditLogRepository auditLog)
        => _auditLog = auditLog;

    /// <summary>
    /// Returns all audit entries for the booking with the given <paramref name="bookingId"/>,
    /// ordered by ChangedAt ascending.
    /// </summary>
    public async Task<IEnumerable<AuditLogDto>> Handle(int bookingId)
    {
        var logs = await _auditLog.GetByEntityAsync("Booking", bookingId);

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