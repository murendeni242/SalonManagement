using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.CreateBookingHandlerTests.Success_Cases
{
    public class CreateBookingHandlerTests_HappyPath
    {
        // ── Shared setup ──────────────────────────────────────────────
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<IStaffScheduleRepository> _scheduleRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateBookingHandler _handler;

        private readonly Service _service;

        // BookingDate is a Monday — matches the default schedule below
        private static readonly DateTime BookingDate = new(2026, 6, 15);

        public CreateBookingHandlerTests_HappyPath()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("reception@salon.co.za");

            _service = new ServiceBuilder()
                .WithId(1)
                .WithDuration(60)
                .WithBasePrice(280m)
                .Build();

            _serviceRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(_service);

            // Default — staff works Monday 08:00–18:00 (covers all test slots)
            var defaultSchedule = new StaffSchedule(
                staffId: 2,
                dayOfWeek: DayOfWeek.Monday,
                startTime: new TimeSpan(8, 0, 0),
                endTime: new TimeSpan(18, 0, 0));

            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(2, DayOfWeek.Monday))
                .ReturnsAsync(defaultSchedule);

            // Default — no overlap
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(false);

            _bookingRepo
                .Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            _handler = new CreateBookingHandler(
                _bookingRepo.Object,
                _serviceRepo.Object,
                _auditLog.Object,
                _currentUser.Object,
                _scheduleRepo.Object);
        }

        // ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsBookingDto()
        {
            // Arrange
            var command = BuildCommand(startHour: 10);

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.Should().NotBeNull();
            result.StaffId.Should().Be(2);
            result.ServiceId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_EndTimeCalculatedFromServiceDuration()
        {
            // Arrange — 60-minute service starting at 10:00 → end must be 11:00
            var command = BuildCommand(startHour: 10);

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.EndTime.Should().Be(new TimeSpan(11, 0, 0));
        }

        [Fact]
        public async Task Handle_90MinuteService_EndTimeCalculatedCorrectly()
        {
            // Arrange — 90-minute service starting at 09:00 → end must be 10:30
            var longService = new ServiceBuilder()
                .WithId(2)
                .WithDuration(90)
                .WithBasePrice(450m)
                .Build();

            _serviceRepo
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(longService);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 2,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(9, 0, 0)
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.EndTime.Should().Be(new TimeSpan(10, 30, 0));
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsyncOnce()
        {
            // Arrange
            var command = BuildCommand(startHour: 10);

            // Act
            await _handler.Handle(command);

            // Assert
            _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithCreatedAction()
        {
            // Arrange
            var command = BuildCommand(startHour: 10);

            // Act
            await _handler.Handle(command);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Created" &&
                           log.EntityName == "Booking")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditLogContainsUserEmail()
        {
            // Arrange
            var command = BuildCommand(startHour: 10);

            // Act
            await _handler.Handle(command);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.ChangedBy == "reception@salon.co.za")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PriceTakenFromService()
        {
            // Arrange — service base price is R280
            var command = BuildCommand(startHour: 10);

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.TotalPrice.Should().Be(280m);
        }

        [Theory]
        [InlineData(11, 0)]  // starts exactly when existing ends   → adjacent, allowed
        [InlineData(8, 0)]  // ends exactly when existing starts   → adjacent, allowed
        public async Task Handle_AdjacentSlots_DoNotConflict(int startHour, int startMinute)
        {
            // Arrange — repository confirms no overlap for adjacent slots
            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(startHour, startMinute, 0)
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should().NotThrowAsync();
        }

        // ── Private helpers ───────────────────────────────────────────

        private static CreateBookingCommand BuildCommand(int startHour) => new()
        {
            CustomerId = 1,
            StaffId = 2,
            ServiceId = 1,
            BookingDate = BookingDate,
            StartTime = new TimeSpan(startHour, 0, 0)
        };
    }
}
