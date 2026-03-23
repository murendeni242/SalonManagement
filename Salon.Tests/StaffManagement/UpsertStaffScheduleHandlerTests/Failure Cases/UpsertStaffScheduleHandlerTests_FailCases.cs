using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffSchedules;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.StaffManagement.UpsertStaffScheduleHandlerTests.Failure_Cases
{
    public class UpsertStaffScheduleHandlerTests_FailCases
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IStaffScheduleRepository> _scheduleRepo = new();
        private readonly UpsertStaffScheduleHandler _handler;

        public UpsertStaffScheduleHandlerTests_FailCases()
        {
            _handler = new UpsertStaffScheduleHandler(
                _staffRepo.Object,
                _scheduleRepo.Object);
        }

        [Fact]
        public async Task Handle_StaffNotFound_ThrowsNotFoundException()
        {
            _staffRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Staff?)null);

            var act = () => _handler.Handle(new UpsertStaffScheduleCommand
            {
                StaffId = 99,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0)
            });

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Staff*");
        }

        [Fact]
        public async Task Handle_StaffNotFound_NeverChecksScheduleRepo()
        {
            _staffRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Staff?)null);

            try
            {
                await _handler.Handle(new UpsertStaffScheduleCommand
                {
                    StaffId = 99,
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0)
                });
            }
            catch { /* expected */ }

            _scheduleRepo.Verify(
                r => r.GetByStaffIdAndDayAsync(It.IsAny<int>(), It.IsAny<DayOfWeek>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EndTimeBeforeStartTime_ThrowsDomainException()
        {
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(1, DayOfWeek.Monday))
                .ReturnsAsync((StaffSchedule?)null);

            var act = () => _handler.Handle(new UpsertStaffScheduleCommand
            {
                StaffId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(17, 0, 0),
                EndTime = new TimeSpan(9, 0, 0)
            });

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("*End time must be later than start time*");
        }
    }
}
