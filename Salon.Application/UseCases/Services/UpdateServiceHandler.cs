using System.Text.Json;
using Salon.Application.DTOs.Services;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Services;

/// <summary>
/// Updates an existing service and writes an "Updated" entry to the shared audit log
/// with a before/after JSON snapshot — same pattern as UpdateBookingHandler.
/// </summary>
public class UpdateServiceHandler
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public UpdateServiceHandler(
        IServiceRepository serviceRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _serviceRepository = serviceRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Workflow: load service → snapshot old state → apply changes → save → write audit → return DTO.
    /// </summary>
    /// <param name="command">Validated input from the controller.</param>
    /// <returns>The updated service as a DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when the service ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when the service is deleted or a rule is violated.</exception>
    public async Task<ServiceDto> Handle(UpdateServiceCommand command)
    {
        var service = await _serviceRepository.GetByIdAsync(command.Id)
            ?? throw new NotFoundException("Service", command.Id);

        // Snapshot BEFORE state for the audit log
        var oldSnapshot = Snapshot(service);

        // Apply changes through domain entity — enforces all business rules
        service.Update(
            command.Name,
            command.DurationMinutes,
            command.BasePrice,
            command.Description ?? string.Empty);

        service.SetStatus(command.Status);

        await _serviceRepository.UpdateAsync(service);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Service",
            entityId: service.Id,
            action: "Updated",
            description: $"Service '{service.Name}' updated by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            oldValues: oldSnapshot,
            newValues: Snapshot(service)));

        return ToDto(service);
    }

    private static string Snapshot(Service s) =>
        JsonSerializer.Serialize(new
        { s.Name, s.Description, s.DurationMinutes, s.BasePrice, s.Status });

    private static ServiceDto ToDto(Service s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        DurationMinutes = s.DurationMinutes,
        BasePrice = s.BasePrice,
        Status = s.Status,
        IsDeleted = s.IsDeleted,
        DeletedAt = s.DeletedAt
    };
}