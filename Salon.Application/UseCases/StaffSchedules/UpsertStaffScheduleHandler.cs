using Salon.Application.DTOs;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.StaffSchedules
{
    /// <summary>
    /// Creates or updates a single working-hours row for one staff member on one day.
    ///
    /// Upsert logic:
    ///   - If a row already exists for (StaffId, DayOfWeek) → call UpdateHours()
    ///   - If no row exists → create a new StaffSchedule and save it
    /// </summary>
    public class UpsertStaffScheduleHandler
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IStaffScheduleRepository _scheduleRepository;

        public UpsertStaffScheduleHandler(
            IStaffRepository staffRepository,
            IStaffScheduleRepository scheduleRepository)
        {
            _staffRepository = staffRepository;
            _scheduleRepository = scheduleRepository;
        }

        /// <summary>
        /// Workflow: verify staff exists → check for existing row → update or create → return DTO.
        /// </summary>
        /// <exception cref="NotFoundException">Thrown when the staff member does not exist.</exception>
        /// <exception cref="DomainException">Thrown when end time is not after start time.</exception>
        public async Task<WorkingHoursDto> Handle(UpsertStaffScheduleCommand command)
        {
            var staff = await _staffRepository.GetByIdAsync(command.StaffId)
                ?? throw new NotFoundException("Staff", command.StaffId);

            var existing = await _scheduleRepository
                .GetByStaffIdAndDayAsync(command.StaffId, command.DayOfWeek);

            if (existing is not null)
            {
                existing.UpdateHours(command.StartTime, command.EndTime);
                await _scheduleRepository.UpdateAsync(existing);
                return GetWeeklyScheduleHandler.ToDto(existing);
            }

            var schedule = new StaffSchedule(
                command.StaffId,
                command.DayOfWeek,
                command.StartTime,
                command.EndTime);

            await _scheduleRepository.AddAsync(schedule);
            return GetWeeklyScheduleHandler.ToDto(schedule);
        }
    }
}
