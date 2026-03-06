using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.CompleteBookingHandlerTests.Failure_Cases
{
    public class CompleteBookingHandlerTests_FailureCases
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CompleteBookingHandler _handler;

        public CompleteBookingHandlerTests_FailureCases()
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
        public async Task Handle_PendingBooking_ThrowsDomainException()
        {
            // Arrange — must confirm before completing
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Pending)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            var act = () => _handler.Handle(1);

            // Assert — domain rule: "Booking must be confirmed first"
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*confirmed*");
        }

        [Theory]
        [InlineData(BookingStatus.Completed)]   // already done — no double complete
        [InlineData(BookingStatus.Cancelled)]   // cancelled — cannot complete
        public async Task Handle_TerminalStatus_ThrowsDomainException(BookingStatus status)
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

            // Assert
            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
