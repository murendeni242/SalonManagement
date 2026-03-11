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
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateBookingHandler _handler;

        // Reusable service — 60-minute haircut at R280
        private readonly Service _service;

        // Booking date used across all tests
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

            _handler = new CreateBookingHandler(
                _bookingRepo.Object,
                _serviceRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_NoOverlap_CreatesBookingSuccessfully()
        {
            // Arrange — no existing bookings conflict
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(false);

            _bookingRepo
                .Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0),  // 10:00
                Notes = null
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert — booking created, end time = start + 60 min
            result.Should().NotBeNull();
            result.EndTime.Should().Be(new TimeSpan(11, 0, 0));
        }

        [Fact]
        public async Task Handle_NoOverlap_CallsAddAsync()
        {
            // Arrange
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(false);

            _bookingRepo
                .Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            // Act
            await _handler.Handle(command);

            // Assert — booking must be persisted exactly once
            _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoOverlap_WritesAuditLogWithCreatedAction()
        {
            // Arrange
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(false);

            _bookingRepo
                .Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

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
        public async Task Handle_NoOverlap_EndTimeCalculatedFromServiceDuration()
        {
            // Arrange — service is 90 minutes, start at 09:00 → end should be 10:30
            var longService = new ServiceBuilder()
                .WithId(2)
                .WithDuration(90)
                .WithBasePrice(450m)
                .Build();

            _serviceRepo
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(longService);

            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(false);

            _bookingRepo
                .Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 2,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(9, 0, 0)   // 09:00
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert — 09:00 + 90 min = 10:30
            result.EndTime.Should().Be(new TimeSpan(10, 30, 0));
        }
    }
}
