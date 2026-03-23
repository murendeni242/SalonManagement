using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffSchedules;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.StaffManagement.GetWeeklyScheduleHandlerTests.Failure_Cases
{
    public class GetWeeklyScheduleHandlerTests_FailCases
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IStaffScheduleRepository> _scheduleRepo = new();
        private readonly GetWeeklyScheduleHandler _handler;

        public GetWeeklyScheduleHandlerTests_FailCases()
            => _handler = new GetWeeklyScheduleHandler(
                _staffRepo.Object, _scheduleRepo.Object);

        [Fact]
        public async Task Handle_StaffNotFound_ThrowsNotFoundException()
        {
            _staffRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Staff?)null);

            var act = () => _handler.Handle(99);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Staff*");
        }

        [Fact]
        public async Task Handle_StaffNotFound_NeverQueriesScheduleRepo()
        {
            _staffRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Staff?)null);

            try { await _handler.Handle(99); } catch { /* expected */ }

            _scheduleRepo.Verify(
                r => r.GetByStaffIdAsync(It.IsAny<int>()),
                Times.Never);
        }
    }
}
