using Salon.Application.DTOs;
using Salon.Application.DTOs.Staff;
using Salon.Domain.Common;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.StaffManagement;

/// <summary>
/// Returns the daily schedule for a specific staff member.
///
/// This is one of the most important queries in the salon system:
/// - The Owner uses it to see what each stylist is doing each day.
/// - The Staff role uses it to see their own appointments for the day.
/// - The booking calendar is built from this data.
///
/// Returns all non-cancelled bookings for the staff member on the given date,
/// ordered by StartTime ascending so appointments read top-to-bottom.
/// </summary>
public class GetStaffScheduleHandler
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffScheduleHandler(IStaffRepository staffRepository)
        => _staffRepository = staffRepository;

    /// <summary>
    /// Returns the schedule for <paramref name="staffId"/> on <paramref name="date"/>.
    /// </summary>
    /// <param name="staffId">Primary key of the staff member.</param>
    /// <param name="date">The calendar date to retrieve the schedule for.</param>
    /// <returns>A <see cref="StaffScheduleDto"/> with the appointments list for the day.</returns>
    /// <exception cref="NotFoundException">Thrown when the staff ID does not exist.</exception>
    public async Task<StaffScheduleDto> Handle(int staffId, DateTime date)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId)
            ?? throw new NotFoundException("Staff", staffId);

        var bookings = await _staffRepository.GetScheduleAsync(staffId, date.Date);

        var appointments = bookings.Select(b => new StaffScheduleItemDto
        {
            BookingId = b.Id,
            CustomerId = b.CustomerId,
            ServiceId = b.ServiceId,
            BookingDate = b.BookingDate,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            Status = b.Status.ToString(),
            Notes = b.Notes
        }).ToList();

        return new StaffScheduleDto
        {
            StaffId = staff.Id,
            StaffName = staff.FullName,
            Date = date.Date,
            Appointments = appointments
        };
    }
}

/// <summary>
/// Returns the full audit history for a staff record, oldest first.
/// Uses the same shared IAuditLogRepository as bookings, services, and sales.
/// entityName = "Staff". Owner role only.
/// </summary>
public class GetStaffAuditLogsHandler
{
    private readonly IAuditLogRepository _auditLog;

    public GetStaffAuditLogsHandler(IAuditLogRepository auditLog)
        => _auditLog = auditLog;

    /// <summary>
    /// Returns all audit entries for the staff member with the given <paramref name="staffId"/>,
    /// ordered by ChangedAt ascending.
    /// </summary>
    public async Task<IEnumerable<AuditLogDto>> Handle(int staffId)
    {
        var logs = await _auditLog.GetByEntityAsync("Staff", staffId);

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