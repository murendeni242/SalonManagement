using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.StaffManagement;

/// <summary>
/// Soft-deletes a staff profile and writes a "SoftDeleted" audit entry.
/// The row stays in the database so historical bookings and the audit trail are never broken.
/// Same pattern as DeleteServiceHandler and DeleteBookingHandler.
/// Owner role only.
/// </summary>
public class DeleteStaffHandler
{
    private readonly IStaffRepository _staffRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public DeleteStaffHandler(
        IStaffRepository staffRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _staffRepository = staffRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Soft-deletes the staff member with the given <paramref name="staffId"/>.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the staff ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when already deleted.</exception>
    public async Task Handle(int staffId)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId)
            ?? throw new NotFoundException("Staff", staffId);

        staff.SoftDelete();
        await _staffRepository.UpdateAsync(staff);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Staff",
            entityId: staff.Id,
            action: "SoftDeleted",
            description: $"Staff profile '{staff.FullName}' deleted by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail));
    }
}