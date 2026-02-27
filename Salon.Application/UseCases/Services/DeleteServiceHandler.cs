using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Services;

/// <summary>
/// Soft-deletes a service and writes a "SoftDeleted" entry to the shared audit log.
/// The row stays in the database so historical bookings and the audit trail are never broken.
/// Same pattern as DeleteBookingHandler. Owner role only.
/// </summary>
public class DeleteServiceHandler
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public DeleteServiceHandler(
        IServiceRepository serviceRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _serviceRepository = serviceRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Soft-deletes the service with the given <paramref name="serviceId"/>.
    /// </summary>
    /// <param name="serviceId">Primary key of the service to delete.</param>
    /// <exception cref="NotFoundException">Thrown when the service does not exist.</exception>
    /// <exception cref="DomainException">Thrown when already deleted.</exception>
    public async Task Handle(int serviceId)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId)
            ?? throw new NotFoundException("Service", serviceId);

        service.SoftDelete();
        await _serviceRepository.UpdateAsync(service);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Service",
            entityId: service.Id,
            action: "SoftDeleted",
            description: $"Service '{service.Name}' deleted by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail));
    }
}