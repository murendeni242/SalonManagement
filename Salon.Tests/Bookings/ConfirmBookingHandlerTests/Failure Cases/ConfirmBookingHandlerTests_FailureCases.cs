using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.ConfirmBookingHandlerTests.Failure_Cases
{
    public class ConfirmBookingHandlerTests_FailureCases
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly ConfirmBookingHandler _handler;

        public ConfirmBookingHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("reception@salon.co.za");
            _handler = new ConfirmBookingHandler(_bookingRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_BookingNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _bookingRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Booking?)null);

            // Act
            var act = () => _handler.Handle(99);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Booking*");
        }

        [Theory]
        [InlineData(BookingStatus.Confirmed)]   // already confirmed
        [InlineData(BookingStatus.Completed)]   // already done
        [InlineData(BookingStatus.Cancelled)]   // already cancelled
        public async Task Handle_NonPendingBooking_ThrowsDomainException(BookingStatus status)
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(status)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            var act = () => _handler.Handle(1);

            // Assert — domain rule violation for any non-Pending status
            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
