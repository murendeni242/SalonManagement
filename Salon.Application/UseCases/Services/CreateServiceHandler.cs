using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Services;

/// <summary>
/// Creates a new service and writes a "Created" entry to the shared audit log.
/// Same pattern as CreateBookingHandler.
/// </summary>
public class CreateServiceHandler
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public CreateServiceHandler(
        IServiceRepository serviceRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _serviceRepository = serviceRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Creates a service, saves it, writes an audit entry, returns the new ID.
    /// </summary>
    public async Task<int> Handle(CreateServiceCommand command)
    {
        var service = new Service(
            command.Name,
            command.DurationMinutes,
            command.BasePrice,
            command.Description ?? string.Empty);

        await _serviceRepository.AddAsync(service);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Service",
            entityId: service.Id,
            action: "Created",
            description: $"Service '{service.Name}' created by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            newValues: Snapshot(service)));

        return service.Id;
    }

    private static string Snapshot(Service s) =>
        System.Text.Json.JsonSerializer.Serialize(new
        { s.Name, s.Description, s.DurationMinutes, s.BasePrice, s.Status });
}