using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.CreateBookingHandlerTests.Failure_Cases
{
    public class CreateBookingHandlerTests_FailureCases
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

        public CreateBookingHandlerTests_FailureCases()
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
        public async Task Handle_OverlapCaseA_NewStartInsideExistingSlot_ThrowsDomainException()
        {
            // Existing:  10:00 → 11:00
            // New:       10:30 → 11:30  ← starts inside existing slot
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    2, BookingDate,
                    new TimeSpan(10, 30, 0),
                    new TimeSpan(11, 30, 0), null))
                .ReturnsAsync(true);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 30, 0)
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already booked*");
        }

        [Fact]
        public async Task Handle_OverlapCaseB_NewEndInsideExistingSlot_ThrowsDomainException()
        {
            // Existing:  10:00 → 11:00
            // New:       09:30 → 10:30  ← ends inside existing slot
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    2, BookingDate,
                    new TimeSpan(9, 30, 0),
                    new TimeSpan(10, 30, 0), null))
                .ReturnsAsync(true);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(9, 30, 0)
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already booked*");
        }

        [Fact]
        public async Task Handle_OverlapCaseC_NewWindowWrapsExistingSlot_ThrowsDomainException()
        {
            // Existing:  10:00 → 11:00
            // New:       09:00 → 12:00  ← completely wraps existing slot
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    2, BookingDate,
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(10, 0, 0), null))  // endTime set by 60-min service
                .ReturnsAsync(true);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(9, 0, 0)
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already booked*");
        }

        [Fact]
        public async Task Handle_OverlapExists_NeverCallsAddAsync()
        {
            // Arrange — conflict detected
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(true);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            // Act
            try { await _handler.Handle(command); } catch { /* expected */ }

            // Assert — booking must NOT be saved when overlap exists
            _bookingRepo.Verify(
                r => r.AddAsync(It.IsAny<Booking>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_OverlapExists_NeverWritesAuditLog()
        {
            // Arrange — conflict detected
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(true);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            // Act
            try { await _handler.Handle(command); } catch { /* expected */ }

            // Assert — no audit entry should be written for a rejected booking
            _auditLog.Verify(
                a => a.AddAsync(It.IsAny<AuditLog>()),
                Times.Never);
        }

        // ── Non-overlap scenarios — must NOT throw ────────────────────

        [Theory]
        [InlineData(11, 0)]   // Case D — entirely after: new starts exactly when existing ends
        [InlineData(8, 0)]    // Case E — entirely before: new ends exactly when existing starts
        public async Task Handle_AdjacentSlots_DoNotConflict(int startHour, int startMinute)
        {
            // Arrange — repository returns false (no overlap) for adjacent slots
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
                StartTime = new TimeSpan(startHour, startMinute, 0)
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — adjacent bookings are allowed
            await act.Should().NotThrowAsync();
        }

        // ── Service not found ─────────────────────────────────────────

        [Fact]
        public async Task Handle_ServiceNotFound_ThrowsNotFoundException()
        {
            // Arrange — service does not exist
            _serviceRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Service?)null);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 99,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Service*");
        }

        [Fact]
        public async Task Handle_ServiceNotFound_NeverChecksForOverlap()
        {
            // Arrange — service lookup fails before overlap check
            _serviceRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Service?)null);

            var command = new CreateBookingCommand
            {
                CustomerId = 1,
                StaffId = 2,
                ServiceId = 99,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            // Act
            try { await _handler.Handle(command); } catch { /* expected */ }

            // Assert — overlap check must not run if service does not exist
            _bookingRepo.Verify(
                r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null),
                Times.Never);
        }
    }
}
