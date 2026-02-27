using Salon.Application.DTOs;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Customers
{
    /// <summary>
    /// Returns the full audit history for a customer record, oldest first.
    /// Same shared IAuditLogRepository — entityName = "Customer".
    /// Owner role only.
    /// </summary>
    public class GetCustomerAuditLogsHandler
    {
        private readonly IAuditLogRepository _auditLog;

        public GetCustomerAuditLogsHandler(IAuditLogRepository auditLog)
            => _auditLog = auditLog;

        public async Task<IEnumerable<AuditLogDto>> Handle(int customerId)
        {
            var logs = await _auditLog.GetByEntityAsync("Customer", customerId);
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
}
