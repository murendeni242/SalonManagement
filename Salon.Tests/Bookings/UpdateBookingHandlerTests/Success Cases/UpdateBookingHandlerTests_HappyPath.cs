using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.UpdateBookingHandlerTests.Success_Cases
{
    public class UpdateBookingHandlerTests_HappyPath
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

        public UpdateBookingHandlerTests_HappyPath()
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

            // Default — no overlap for this booking's slot
            _bookingRepo
                .Setup(r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(),
                    5))
                .ReturnsAsync(false);

            _bookingRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Booking>()))
                .Returns(Task.CompletedTask);

            _handler = new UpdateBookingHandler(
                _bookingRepo.Object,
                _serviceRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        // ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedBookingDto()
        {
            // Arrange
            var command = BuildCommand(startHour: 14);

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_EndTimeCalculatedFromServiceDuration()
        {
            // Arrange — 60-minute service starting at 14:00 → end must be 15:00
            var command = BuildCommand(startHour: 14);

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.EndTime.Should().Be(new TimeSpan(15, 0, 0));
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            // Arrange
            var command = BuildCommand(startHour: 14);

            // Act
            await _handler.Handle(command);

            // Assert
            _bookingRepo.Verify(r => r.UpdateAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PassesExcludeBookingIdToRepository()
        {
            // Arrange — the booking's own ID must be excluded so it does not
            // conflict with its own current slot
            var command = BuildCommand(startHour: 14);

            // Act
            await _handler.Handle(command);

            // Assert
            _bookingRepo.Verify(
                r => r.ExistsOverlappingBookingAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(),
                    It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(),
                    5),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithUpdatedAction()
        {
            // Arrange
            var command = BuildCommand(startHour: 14);

            // Act
            await _handler.Handle(command);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Updated" &&
                           log.EntityName == "Booking")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditLogContainsBothOldAndNewSnapshots()
        {
            // Arrange
            var command = BuildCommand(startHour: 14);

            // Act
            await _handler.Handle(command);

            // Assert — both snapshots required for full traceability
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.OldValues != null &&
                           log.NewValues != null)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SameSlotNoRealChange_DoesNotConflictWithItself()
        {
            // Arrange — booking keeps the exact same time slot (e.g. only notes changed)
            // excludeBookingId=5 filters out its own slot so no false conflict
            var command = new UpdateBookingCommand
            {
                Id = 5,
                StaffId = 2,
                ServiceId = 1,
                BookingDate = BookingDate,
                StartTime = new TimeSpan(10, 0, 0),
                Notes = "Minor note change only"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — a booking must never block its own rescheduling
            await act.Should().NotThrowAsync();
        }

        // ── Private helpers ───────────────────────────────────────────

        private static UpdateBookingCommand BuildCommand(int startHour) => new()
        {
            Id = 5,
            StaffId = 2,
            ServiceId = 1,
            BookingDate = BookingDate,
            StartTime = new TimeSpan(startHour, 0, 0),
            Notes = "Updated notes"
        };
    }
}
