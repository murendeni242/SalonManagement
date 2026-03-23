using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffSchedules;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.StaffManagement.DeleteStaffScheduleHandlerTests.Failure_Cases
{
    public class DeleteStaffScheduleHandlerTests_FailCases
    {
        private readonly Mock<IStaffScheduleRepository> _scheduleRepo = new();
        private readonly DeleteStaffScheduleHandler _handler;

        public DeleteStaffScheduleHandlerTests_FailCases()
            => _handler = new DeleteStaffScheduleHandler(_scheduleRepo.Object);

        [Fact]
        public async Task Handle_NoRowForThatDay_ThrowsNotFoundException()
        {
            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Sunday))
                .ReturnsAsync((StaffSchedule?)null);

            var act = () => _handler.Handle(
                new DeleteStaffScheduleCommand { StaffId = 1, DayOfWeek = DayOfWeek.Sunday });

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_NoRowForThatDay_NeverCallsDeleteAsync()
        {
            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Sunday))
                .ReturnsAsync((StaffSchedule?)null);

            try
            {
                await _handler.Handle(
                new DeleteStaffScheduleCommand { StaffId = 1, DayOfWeek = DayOfWeek.Sunday });
            }
            catch { /* expected */ }

            _scheduleRepo.Verify(
                r => r.DeleteAsync(It.IsAny<StaffSchedule>()),
                Times.Never);
        }
    }
}
