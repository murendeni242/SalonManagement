using System.Text.Json;
using Salon.Application.DTOs.Staff;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.StaffManagement;

/// <summary>
/// Updates an existing staff profile and writes an "Updated" entry to the shared audit log
/// with a before/after JSON snapshot. Same pattern as UpdateBookingHandler and UpdateServiceHandler.
/// Owner role only.
/// </summary>
public class UpdateStaffHandler
{
    private readonly IStaffRepository _staffRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public UpdateStaffHandler(
        IStaffRepository staffRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _staffRepository = staffRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Workflow: load staff → snapshot old → apply changes → save → write audit → return DTO.
    /// </summary>
    /// <param name="command">Validated input from the controller.</param>
    /// <returns>The updated staff profile as a DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when the staff ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when the record is deleted or a rule is violated.</exception>
    public async Task<StaffDto> Handle(UpdateStaffCommand command)
    {
        var staff = await _staffRepository.GetByIdAsync(command.Id)
            ?? throw new NotFoundException("Staff", command.Id);

        var oldSnapshot = CreateStaffHandler.Snapshot(staff);

        staff.Update(
            command.FirstName,
            command.LastName,
            command.Phone,
            command.Role,
            command.Email);

        staff.SetStatus(command.Status);
        staff.SetSpecialisations(command.Specialisations);

        await _staffRepository.UpdateAsync(staff);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Staff",
            entityId: staff.Id,
            action: "Updated",
            description: $"Staff profile '{staff.FullName}' updated by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            oldValues: oldSnapshot,
            newValues: CreateStaffHandler.Snapshot(staff)));

        return CreateStaffHandler.ToDto(staff);
    }
}