using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.CompleteBookingHandlerTests.Success_Cases
{
    public class CompleteBookingHandlerTests_HappyPath
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CompleteBookingHandler _handler;

        public CompleteBookingHandlerTests_HappyPath()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("owner@salon.co.za");

            _handler = new CompleteBookingHandler(
                _bookingRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ConfirmedBooking_SetsStatusToCompleted()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Confirmed)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(1);

            // Assert
            booking.Status.Should().Be(BookingStatus.Completed);
        }

        [Fact]
        public async Task Handle_ConfirmedBooking_PersistsChange()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Confirmed)
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
        public async Task Handle_ConfirmedBooking_WritesAuditLog()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Confirmed)
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