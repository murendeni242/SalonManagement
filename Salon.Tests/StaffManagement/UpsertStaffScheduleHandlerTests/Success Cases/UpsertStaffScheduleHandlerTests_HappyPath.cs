using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffSchedules;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.StaffManagement.UpsertStaffScheduleHandlerTests.Success_Cases
{
    public class UpsertStaffScheduleHandlerTests_HappyPath
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IStaffScheduleRepository> _scheduleRepo = new();
        private readonly UpsertStaffScheduleHandler _handler;

        public UpsertStaffScheduleHandlerTests_HappyPath()
        {
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(staff);

            _handler = new UpsertStaffScheduleHandler(
                _staffRepo.Object,
                _scheduleRepo.Object);
        }

        [Fact]
        public async Task Handle_NoExistingRow_CreatesNewSchedule()
        {
            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Monday))
                .ReturnsAsync((StaffSchedule?)null);

            _scheduleRepo
                .Setup(r => r.AddAsync(It.IsAny<StaffSchedule>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(BuildCommand(DayOfWeek.Monday, 9, 17));

            result.DayOfWeek.Should().Be(DayOfWeek.Monday);
            result.StartTime.Should().Be("09:00");
            result.EndTime.Should().Be("17:00");
        }

        [Fact]
        public async Task Handle_NoExistingRow_CallsAddAsyncNotUpdateAsync()
        {
            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Monday))
                .ReturnsAsync((StaffSchedule?)null);

            _scheduleRepo
                .Setup(r => r.AddAsync(It.IsAny<StaffSchedule>()))
                .Returns(Task.CompletedTask);

            await _handler.Handle(BuildCommand(DayOfWeek.Monday, 9, 17));

            _scheduleRepo.Verify(r => r.AddAsync(It.IsAny<StaffSchedule>()), Times.Once);
            _scheduleRepo.Verify(r => r.UpdateAsync(It.IsAny<StaffSchedule>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingRow_UpdatesHours()
        {
            var existing = new StaffSchedule(1, DayOfWeek.Monday,
                new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Monday))
                .ReturnsAsync(existing);

            _scheduleRepo
                .Setup(r => r.UpdateAsync(existing))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(BuildCommand(DayOfWeek.Monday, 8, 16));

            result.StartTime.Should().Be("08:00");
            result.EndTime.Should().Be("16:00");
        }

        [Fact]
        public async Task Handle_ExistingRow_CallsUpdateAsyncNotAddAsync()
        {
            var existing = new StaffSchedule(1, DayOfWeek.Monday,
                new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0));

            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Monday))
                .ReturnsAsync(existing);

            _scheduleRepo
                .Setup(r => r.UpdateAsync(existing))
                .Returns(Task.CompletedTask);

            await _handler.Handle(BuildCommand(DayOfWeek.Monday, 8, 16));

            _scheduleRepo.Verify(r => r.UpdateAsync(It.IsAny<StaffSchedule>()), Times.Once);
            _scheduleRepo.Verify(r => r.AddAsync(It.IsAny<StaffSchedule>()), Times.Never);
        }

        private static UpsertStaffScheduleCommand BuildCommand(
            DayOfWeek day, int startHour, int endHour) => new()
            {
                StaffId = 1,
                DayOfWeek = day,
                StartTime = new TimeSpan(startHour, 0, 0),
                EndTime = new TimeSpan(endHour, 0, 0)
            };
    }

}
