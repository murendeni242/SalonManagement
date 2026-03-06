using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.DeleteBookingHandlerTests.Failure_Cases
{
    public class DeleteBookingHandlerTests_FailureCases
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteBookingHandler _handler;

        public DeleteBookingHandlerTests_FailureCases()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("owner@salon.co.za");

            _handler = new DeleteBookingHandler(
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
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Booking*");
        }

    }
}
