using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.CancelBookingHandlerTests.Failure_Cases
{
    public class CancelBookingHandlerTests_FailureCases
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CancelBookingHandler _handler;

        public CancelBookingHandlerTests_FailureCases()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("reception@salon.co.za");

            _handler = new CancelBookingHandler(
                _bookingRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
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
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_CompletedBooking_ThrowsDomainException()
        {
            // Arrange — completed bookings are locked; payment already taken
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Completed)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            var act = () => _handler.Handle(1);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*Completed*");
        }

        [Fact]
        public async Task Handle_AlreadyCancelledBooking_ThrowsDomainException()
        {
            // Arrange — cannot cancel twice
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Cancelled)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            var act = () => _handler.Handle(1);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already cancelled*");
        }
    }

}
