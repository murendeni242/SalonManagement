using Salon.Domain.Common;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.StaffSchedules
{
    /// <summary>
    /// Removes a working-hours row — the staff member no longer works on that day.
    /// Hard delete — no audit trail needed for schedule configuration.
    /// </summary>
    public class DeleteStaffScheduleHandler
    {
        private readonly IStaffScheduleRepository _scheduleRepository;

        public DeleteStaffScheduleHandler(IStaffScheduleRepository scheduleRepository)
            => _scheduleRepository = scheduleRepository;

        /// <summary>
        /// Workflow: find the schedule row → hard delete it.
        /// </summary>
        /// <exception cref="NotFoundException">
        /// Thrown when no schedule row exists for this staff member on this day.
        /// </exception>
        public async Task Handle(DeleteStaffScheduleCommand command)
        {
            var schedule = await _scheduleRepository
                .GetByStaffIdAndDayAsync(command.StaffId, command.DayOfWeek)
                ?? throw new NotFoundException(
                    $"No schedule found for staff {command.StaffId} on {command.DayOfWeek}.");

            await _scheduleRepository.DeleteAsync(schedule);
        }
    }
}
