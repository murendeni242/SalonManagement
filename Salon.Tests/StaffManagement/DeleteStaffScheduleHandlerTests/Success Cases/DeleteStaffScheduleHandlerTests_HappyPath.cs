using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffSchedules;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.StaffManagement.DeleteStaffScheduleHandlerTests.Success_Cases
{
    public class DeleteStaffScheduleHandlerTests_HappyPath
    {
        private readonly Mock<IStaffScheduleRepository> _scheduleRepo = new();
        private readonly DeleteStaffScheduleHandler _handler;

        public DeleteStaffScheduleHandlerTests_HappyPath()
            => _handler = new DeleteStaffScheduleHandler(_scheduleRepo.Object);

        [Fact]
        public async Task Handle_ExistingRow_DeletesSuccessfully()
        {
            var schedule = new StaffSchedule(1, DayOfWeek.Monday,
                new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Monday))
                .ReturnsAsync(schedule);

            _scheduleRepo
                .Setup(r => r.DeleteAsync(schedule))
                .Returns(Task.CompletedTask);

            var act = () => _handler.Handle(
                new DeleteStaffScheduleCommand { StaffId = 1, DayOfWeek = DayOfWeek.Monday });

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_ExistingRow_CallsDeleteAsyncOnce()
        {
            var schedule = new StaffSchedule(1, DayOfWeek.Monday,
                new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Monday))
                .ReturnsAsync(schedule);

            _scheduleRepo
                .Setup(r => r.DeleteAsync(schedule))
                .Returns(Task.CompletedTask);

            await _handler.Handle(
                new DeleteStaffScheduleCommand { StaffId = 1, DayOfWeek = DayOfWeek.Monday });

            _scheduleRepo.Verify(r => r.DeleteAsync(schedule), Times.Once);
        }
    }
}

