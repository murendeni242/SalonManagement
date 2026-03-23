using Salon.Application.DTOs;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.StaffSchedules;

/// <summary>
/// Returns the full weekly working schedule for a staff member.
/// Used by the frontend to render the availability grid.
/// </summary>
public class GetWeeklyScheduleHandler
{
    private readonly IStaffRepository _staffRepository;
    private readonly IStaffScheduleRepository _scheduleRepository;

    public GetWeeklyScheduleHandler(
        IStaffRepository staffRepository,
        IStaffScheduleRepository scheduleRepository)
    {
        _staffRepository = staffRepository;
        _scheduleRepository = scheduleRepository;
    }

    /// <summary>
    /// Workflow: load staff → load schedule rows → map to DTO.
    /// </summary>
    /// <param name="staffId">Primary key of the staff member.</param>
    /// <returns>Full weekly schedule including staff name and all configured working days.</returns>
    /// <exception cref="NotFoundException">Thrown when the staff member does not exist.</exception>
    public async Task<WeeklyWorkingHoursDto> Handle(int staffId)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId)
            ?? throw new NotFoundException("Staff", staffId);

        var rows = await _scheduleRepository.GetByStaffIdAsync(staffId);

        return new WeeklyWorkingHoursDto
        {
            StaffId = staff.Id,
            StaffName = staff.FullName,
            WorkingDays = rows.Select(ToDto).ToList()
        };
    }

    internal static WorkingHoursDto ToDto(StaffSchedule s) => new()
    {
        Id = s.Id,
        StaffId = s.StaffId,
        DayOfWeek = s.DayOfWeek,
        DayName = s.DayOfWeek.ToString(),
        StartTime = s.StartTime.ToString(@"hh\:mm"),
        EndTime = s.EndTime.ToString(@"hh\:mm")
    };
}