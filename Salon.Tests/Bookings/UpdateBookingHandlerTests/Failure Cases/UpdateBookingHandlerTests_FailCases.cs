using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.UpdateBookingHandlerTests.Failure_Cases
{
    public class UpdateBookingHandlerTests_FailCases
    {
        // ── Shared setup ──────────────────────────────────────────────
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly UpdateBookingHandler _handler;

        private readonly Service _service;
        private readonly Booking _booking;
        private static readonly DateTime BookingDate = new(2026, 6, 15);

        public UpdateBookingHandlerTests_FailCases()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("reception@salon.co.za");

            _service = new ServiceBuilder()
                .WithId(1)
                .WithDuration(60)
                .WithBasePrice(280m)
                .Build();

            _booking = new BookingBuilder()
                .WithId(5)
                .WithStaffId(2)
                .Build();

            _serviceRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(_service);

            _bookingRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(_booking);

            _handler = new UpdateBookingHandler(
                _bookingRepo.Object,
                _serviceRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        // ── Overlap with a different booking ──────────────────────────

        [Fact]
        public async Task Handle_OverlapWithDifferentBooking_ThrowsDomainException()
        {
            // Arrange — slot taken by another booking (not booking 5)
            SetupOverlap(returns: true);

            var act = () => _handler.Handle(BuildCommand(startHour: 10));

            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*conflicts*");
        }

        [Fact]
        public async Task Handle_OverlapExists_NeverCallsUpdateAsync()
        {
            // Arrange — rejected updates must never be persisted
            SetupOverlap(returns: true);

            try { await _handler.Handle(BuildCommand(startHour: 10)); } catch { /* expected */ }

            _bookingRepo.Verify(r => r.UpdateAsync(It.IsAny<Booking>()), Times.Never);
        }

        [Fact]
        public async Task Handle_OverlapExists_NeverWritesAuditLog()
        {
            // Arrange — no audit entry should be written for a rejected update
            SetupOverlap(returns: true);

            try { await _handler.Handle(BuildCommand(startHour: 10)); } catch { /* expected */ }

            _auditLog.Verify(a => a.AddAsync(It.IsAny<AuditLog>()), Times.Never);
        }

        // ── Booking not found ─────────────────────────────────────────

        [Fact]
        public async Task Handle_BookingNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _bookingRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Booking?)null);

            var command = new UpdateBookingCommand
            {
                Id = 99,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            var act = () => _handler.Handle(command);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Booking*");
        }

        [Fact]
        public async Task Handle_BookingNotFound_NeverChecksForOverlap()
        {
            // Arrange — booking lookup must fail before overlap check is reached
            _bookingRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Booking?)null);

            var command = new UpdateBookingCommand
            {
                Id = 99,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            try { await _handler.Handle(command); } catch { /* expected */ }

            _bookingRepo.Verify(
                r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(),
                    It.IsAny<int?>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_BookingNotFound_NeverCallsUpdateAsync()
        {
            // Arrange
            _bookingRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Booking?)null);

            var command = new UpdateBookingCommand
            {
                Id = 99,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            try { await _handler.Handle(command); } catch { /* expected */ }

            _bookingRepo.Verify(r => r.UpdateAsync(It.IsAny<Booking>()), Times.Never);
        }

        // ── Service not found ─────────────────────────────────────────

        [Fact]
        public async Task Handle_ServiceNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _serviceRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Service?)null);

            var command = new UpdateBookingCommand
            {
                Id = 5,
                StaffId = 2,
                ServiceId = 99,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            var act = () => _handler.Handle(command);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Service*");
        }

        [Fact]
        public async Task Handle_ServiceNotFound_NeverCallsUpdateAsync()
        {
            // Arrange
            _serviceRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Service?)null);

            var command = new UpdateBookingCommand
            {
                Id = 5,
                StaffId = 2,
                ServiceId = 99,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0)
            };

            try { await _handler.Handle(command); } catch { /* expected */ }

            _bookingRepo.Verify(r => r.UpdateAsync(It.IsAny<Booking>()), Times.Never);
        }

        // ── Private helpers ───────────────────────────────────────────

        private void SetupOverlap(bool returns)
        {
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(),
                    5))
                .ReturnsAsync(returns);
        }

        private static UpdateBookingCommand BuildCommand(int startHour) => new()
        {
            Id = 5,
            StaffId = 2,
            ServiceId = 1,
            BookingDate = BookingDate,
            StartTime = new TimeSpan(startHour, 0, 0)
        };
    }
}
