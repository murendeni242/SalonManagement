using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.CreateBookingHandlerTests.Failure_Cases
{
    public class CreateBookingHandlerTests_FailCases
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

        public CreateBookingHandlerTests_FailCases()
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

            // Default — staff works Monday 08:00–18:00 (covers all overlap test slots)
            var defaultSchedule = new StaffSchedule(
                staffId: 2,
                dayOfWeek: DayOfWeek.Monday,
                startTime: new TimeSpan(8, 0, 0),
                endTime: new TimeSpan(18, 0, 0));

            _scheduleRepo
                .Setup(r => r.GetByStaffIdAndDayAsync(2, DayOfWeek.Monday))
                .ReturnsAsync(defaultSchedule);

            _handler = new CreateBookingHandler(
                _bookingRepo.Object,
                _serviceRepo.Object,
                _auditLog.Object,
                _currentUser.Object,
                _scheduleRepo.Object);
        }

        // ── Overlap scenarios ─────────────────────────────────────────

        [Fact]
        public async Task Handle_OverlapCaseA_NewStartInsideExistingSlot_ThrowsDomainException()
        {
            // Existing:  10:00 → 11:00
            // New:       10:30 → 11:30  ← starts inside existing slot
            SetupOverlap(startHour: 10, startMinute: 30, returns: true);

            var act = () => _handler.Handle(BuildCommand(startHour: 10, startMinute: 30));

            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already booked*");
        }

        [Fact]
        public async Task Handle_OverlapCaseB_NewEndInsideExistingSlot_ThrowsDomainException()
        {
            // Existing:  10:00 → 11:00
            // New:       09:30 → 10:30  ← ends inside existing slot
            SetupOverlap(startHour: 9, startMinute: 30, returns: true);

            var act = () => _handler.Handle(BuildCommand(startHour: 9, startMinute: 30));

            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already booked*");
        }

        [Fact]
        public async Task Handle_OverlapCaseC_NewWindowWrapsExistingSlot_ThrowsDomainException()
        {
            // Existing:  10:00 → 11:00
            // New:       09:00 → 12:00  ← completely wraps existing slot
            SetupOverlap(startHour: 9, startMinute: 0, returns: true);

            var act = () => _handler.Handle(BuildCommand(startHour: 9, startMinute: 0));

            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already booked*");
        }

        [Fact]
        public async Task Handle_OverlapExists_NeverCallsAddAsync()
        {
            // Arrange — conflict detected, booking must not be persisted
            SetupAnyOverlap(returns: true);

            try { await _handler.Handle(BuildCommand(10, 0)); } catch { /* expected */ }

            _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }

        [Fact]
        public async Task Handle_OverlapExists_NeverWritesAuditLog()
        {
            // Arrange — conflict detected, no audit entry should be written
            SetupAnyOverlap(returns: true);

            try { await _handler.Handle(BuildCommand(10, 0)); } catch { /* expected */ }

            _auditLog.Verify(a => a.AddAsync(It.IsAny<AuditLog>()), Times.Never);
        }

        // ── Service not found ─────────────────────────────────────────

        [Fact]
        public async Task Handle_ServiceNotFound_ThrowsNotFoundException()
        {
            // Arrange
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

            var act = () => _handler.Handle(command);

            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Service*");
        }

        [Fact]
        public async Task Handle_ServiceNotFound_NeverChecksForOverlap()
        {
            // Arrange — service lookup must fail before overlap check is reached
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

            try { await _handler.Handle(command); } catch { /* expected */ }

            // Overlap check must never run if service does not exist
            _bookingRepo.Verify(
                r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ServiceNotFound_NeverCallsAddAsync()
        {
            // Arrange
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

            try { await _handler.Handle(command); } catch { /* expected */ }

            _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }

        // ── Private helpers ───────────────────────────────────────────

        private void SetupOverlap(int startHour, int startMinute, bool returns)
        {
            var start = new TimeSpan(startHour, startMinute, 0);
            var end = start.Add(TimeSpan.FromMinutes(60));

            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    2, BookingDate, start, end, null))
                .ReturnsAsync(returns);
        }

        private void SetupAnyOverlap(bool returns)
        {
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), null))
                .ReturnsAsync(returns);
        }

        private static CreateBookingCommand BuildCommand(int startHour, int startMinute = 0) => new()
        {
            CustomerId = 1,
            StaffId = 2,
            ServiceId = 1,
            BookingDate = BookingDate,
            StartTime = new TimeSpan(startHour, startMinute, 0)
        };
    }
}
