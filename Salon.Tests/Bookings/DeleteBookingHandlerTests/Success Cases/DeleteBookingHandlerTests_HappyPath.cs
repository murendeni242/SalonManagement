using Moq;
using Salon.Application.UseCases.Bookings;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Bookings.DeleteBookingHandlerTests.Success_Cases
{
    public class DeleteBookingHandlerTests_HappyPath
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteBookingHandler _handler;

        public DeleteBookingHandlerTests_HappyPath()
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
        public async Task Handle_ExistingBooking_CallsDeleteAsync()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(1);

            // Assert — handler must call DeleteAsync, NOT UpdateAsync
            // SalonDbContext intercepts the Delete and converts to soft delete
            _bookingRepo.Verify(
                r => r.DeleteAsync(booking),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingBooking_WritesAuditLogWithSoftDeletedAction()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(1);

            // Assert — audit log entry must have action = "SoftDeleted"
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "SoftDeleted")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingBooking_NeverCallsUpdateAsync()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            // Act
            await _handler.Handle(1);

            // Assert — soft delete must go through DeleteAsync only, never UpdateAsync
            _bookingRepo.Verify(
                r => r.UpdateAsync(It.IsAny<Booking>()),
                Times.Never);
        }
    }
}
