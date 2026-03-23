using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffSchedules;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.StaffManagement.GetWeeklyScheduleHandlerTests.Success_Cases
{
    public class GetWeeklyScheduleHandlerTests_HappyPath
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IStaffScheduleRepository> _scheduleRepo = new();
        private readonly GetWeeklyScheduleHandler _handler;

        public GetWeeklyScheduleHandlerTests_HappyPath()
            => _handler = new GetWeeklyScheduleHandler(
                _staffRepo.Object, _scheduleRepo.Object);

        [Fact]
        public async Task Handle_StaffWithThreeDays_ReturnsAllThreeDays()
        {
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);

            var rows = new List<StaffSchedule>
        {
            new(1, DayOfWeek.Monday,    new TimeSpan(9,  0, 0), new TimeSpan(17, 0, 0)),
            new(1, DayOfWeek.Wednesday, new TimeSpan(10, 0, 0), new TimeSpan(18, 0, 0)),
            new(1, DayOfWeek.Friday,    new TimeSpan(9,  0, 0), new TimeSpan(15, 0, 0))
        };

            _scheduleRepo.Setup(r => r.GetByStaffIdAsync(1)).ReturnsAsync(rows);

            var result = await _handler.Handle(1);

            result.WorkingDays.Should().HaveCount(3);
            result.WorkingDays.Select(d => d.DayOfWeek)
                .Should().Contain(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday });
        }

        [Fact]
        public async Task Handle_StaffWithNoDays_ReturnsEmptyWorkingDays()
        {
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _scheduleRepo
                .Setup(r => r.GetByStaffIdAsync(1))
                .ReturnsAsync(new List<StaffSchedule>());

            var result = await _handler.Handle(1);

            result.WorkingDays.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsStaffNameInDto()
        {
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _scheduleRepo
                .Setup(r => r.GetByStaffIdAsync(1))
                .ReturnsAsync(new List<StaffSchedule>());

            var result = await _handler.Handle(1);

            result.StaffName.Should().NotBeNullOrWhiteSpace();
            result.StaffId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_DayNamePopulatedInDto()
        {
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _scheduleRepo
                .Setup(r => r.GetByStaffIdAsync(1))
                .ReturnsAsync(new List<StaffSchedule>
                {
                new(1, DayOfWeek.Tuesday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
                });

            var result = await _handler.Handle(1);

            result.WorkingDays[0].DayName.Should().Be("Tuesday");
        }
    }
}
