using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.CancelBookingHandlerTests.Success_Cases
{
    public class CancelBookingHandlerTests_HappyPath
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CancelBookingHandler _handler;

        public CancelBookingHandlerTests_HappyPath()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("reception@salon.co.za");

            _handler = new CancelBookingHandler(
                _bookingRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Theory]
        [InlineData(BookingStatus.Pending)]     // not yet confirmed — still cancellable
        [InlineData(BookingStatus.Confirmed)]   // confirmed — still cancellable
        public async Task Handle_ActiveBooking_SetsStatusToCancelled(BookingStatus initialStatus)
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(initialStatus)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(1);

            // Assert
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task Handle_ActiveBooking_PersistsChange()
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
            _bookingRepo.Verify(
                r => r.UpdateAsync(booking),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ActiveBooking_WritesAuditLog()
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
            _auditLog.Verify(
                a => a.AddAsync(It.IsAny<AuditLog>()),
                Times.Once);
        }

    }
}
