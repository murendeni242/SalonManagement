using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffManagement;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.StaffManagement.GetStaffHandlerTests.Success_Cases
{
    public class GetStaffScheduleHandlerTests
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly GetStaffScheduleHandler _handler;

        public GetStaffScheduleHandlerTests()
            => _handler = new GetStaffScheduleHandler(_staffRepo.Object);

        [Fact]
        public async Task Handle_ExistingStaff_ReturnsScheduleWithCorrectStaffName()
        {
            // Arrange
            var staff = new StaffBuilder().WithId(1).WithFirstName("Nomsa").WithLastName("Zulu").Build();
            var date = new DateTime(2025, 6, 15);

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.GetScheduleAsync(1, date.Date)).ReturnsAsync(new List<Booking>());

            // Act
            var result = await _handler.Handle(1, date);

            // Assert
            result.StaffId.Should().Be(1);
            result.StaffName.Should().Be("Nomsa Zulu");
            result.Date.Should().Be(date.Date);
        }

        [Fact]
        public async Task Handle_ExistingStaff_ReturnsAppointmentsMappedFromBookings()
        {
            // Arrange — 2 bookings on the day
            var staff = new StaffBuilder().WithId(1).Build();
            var date = new DateTime(2025, 6, 15);

            var bookings = new List<Booking>
        {
            new BookingBuilder().WithId(1).WithStaffId(1).Build(),
            new BookingBuilder().WithId(2).WithStaffId(1).Build(),
        };

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.GetScheduleAsync(1, date.Date)).ReturnsAsync(bookings);

            // Act
            var result = await _handler.Handle(1, date);

            // Assert
            result.Appointments.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ExistingStaff_PassesDateOnlyToRepository()
        {
            // Arrange — time component must be stripped before querying
            var staff = new StaffBuilder().WithId(1).Build();
            var dateTime = new DateTime(2025, 6, 15, 14, 30, 0);  // has time component

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.GetScheduleAsync(1, dateTime.Date)).ReturnsAsync(new List<Booking>());

            // Act
            await _handler.Handle(1, dateTime);

            // Assert — only the date part must be passed (no time)
            _staffRepo.Verify(
                r => r.GetScheduleAsync(1, dateTime.Date),
                Times.Once);
        }

        [Fact]
        public async Task Handle_StaffNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _staffRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Staff?)null);

            // Act
            var act = () => _handler.Handle(99, DateTime.Today);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Staff*");
        }

        [Fact]
        public async Task Handle_EmptySchedule_ReturnsScheduleWithEmptyAppointments()
        {
            // Arrange — staff exists but has no bookings on this day
            var staff = new StaffBuilder().WithId(1).Build();
            var date = new DateTime(2025, 6, 15);

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.GetScheduleAsync(1, date.Date)).ReturnsAsync(new List<Booking>());

            // Act
            var result = await _handler.Handle(1, date);

            // Assert — returns schedule with empty list, not null
            result.Appointments.Should().NotBeNull();
            result.Appointments.Should().BeEmpty();
        }
    }
}
