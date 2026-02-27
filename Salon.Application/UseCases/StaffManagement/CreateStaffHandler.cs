using Salon.Application.DTOs.Staff;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.StaffManagement;

/// <summary>
/// Creates a new staff profile and writes a "Created" entry to the shared audit log.
/// Same pattern as CreateServiceHandler and CreateBookingHandler.
/// </summary>
public class CreateStaffHandler
{
    private readonly IStaffRepository _staffRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public CreateStaffHandler(
        IStaffRepository staffRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _staffRepository = staffRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Creates the staff profile, sets specialisations, saves, writes audit entry, returns DTO.
    /// </summary>
    /// <param name="command">Validated input from the controller.</param>
    /// <returns>The newly created staff profile as a DTO.</returns>
    public async Task<StaffDto> Handle(CreateStaffCommand command)
    {
        var staff = new Staff(
            command.FirstName,
            command.LastName,
            command.Phone,
            command.Role,
            command.Email);

        if (command.Specialisations.Any())
            staff.SetSpecialisations(command.Specialisations);

        await _staffRepository.AddAsync(staff);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Staff",
            entityId: staff.Id,
            action: "Created",
            description: $"Staff profile '{staff.FullName}' ({staff.Role}) created by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            newValues: Snapshot(staff)));

        return ToDto(staff);
    }

    internal static string Snapshot(Staff s) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            s.FirstName,
            s.LastName,
            s.Phone,
            s.Email,
            s.Role,
            s.Status,
            s.Specialisations
        });

    internal static StaffDto ToDto(Staff s) => new()
    {
        Id = s.Id,
        FirstName = s.FirstName,
        LastName = s.LastName,
        FullName = s.FullName,
        Phone = s.Phone,
        Email = s.Email,
        Role = s.Role,
        Status = s.Status,
        Specialisations = s.GetSpecialisationIds(),
        IsDeleted = s.IsDeleted,
        DeletedAt = s.DeletedAt
    };
}