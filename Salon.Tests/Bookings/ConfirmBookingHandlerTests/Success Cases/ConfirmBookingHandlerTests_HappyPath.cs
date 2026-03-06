using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.ConfirmBookingHandlerTests.Success_Cases
{
    public class ConfirmBookingHandlerTests_HappyPath
    {

        // ── Shared mocks — recreated fresh for every test by xUnit ────
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly ConfirmBookingHandler _handler;

        public ConfirmBookingHandlerTests_HappyPath()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("reception@salon.co.za");

            _handler = new ConfirmBookingHandler(
                _bookingRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_PendingBooking_SetsStatusToConfirmed()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Pending)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(1);

            // Assert
            booking.Status.Should().Be(BookingStatus.Confirmed);
        }

        [Fact]
        public async Task Handle_PendingBooking_CallsUpdateAsync()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Pending)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(1);

            // Assert — booking must be persisted after status change
            _bookingRepo.Verify(
                r => r.UpdateAsync(booking),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PendingBooking_WritesAuditLog()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Pending)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(1);

            // Assert — one audit entry must be written (content not tested here)
            _auditLog.Verify(
                a => a.AddAsync(It.IsAny<AuditLog>()),
                Times.Once);
        }
    }
}
