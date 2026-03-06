
using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Sales;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Sales.CreateSaleHandlerTests.Failure_Cases
{
    public class CreateSaleHandlerTests_FailureCases
    {
        private readonly Mock<ISaleRepository> _saleRepo = new();
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateSaleHandler _handler;

        public CreateSaleHandlerTests_FailureCases()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("reception@salon.co.za");

            _handler = new CreateSaleHandler(
                _saleRepo.Object,
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

            var command = new CreateSaleCommand
            {
                BookingId = 99,
                AmountPaid = 200m,
                PaymentMethod = "Cash"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Booking*");
        }

        [Fact]
        public async Task Handle_CancelledBooking_ThrowsDomainException()
        {
            // Arrange — payments cannot be taken against cancelled bookings
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Cancelled)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            var command = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 200m,
                PaymentMethod = "Cash"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*Cancelled*");
        }

        [Fact]
        public async Task Handle_ZeroAmount_ThrowsDomainException()
        {
            // Arrange — domain entity Sale constructor rejects amount <= 0
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Completed)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            var command = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 0m,   // ← invalid
                PaymentMethod = "Cash"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — DomainException thrown by Sale constructor
            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_InvalidPaymentMethod_ThrowsDomainException()
        {
            // Arrange — domain entity Sale rejects unrecognised payment methods
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Completed)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            var command = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 200m,
                PaymentMethod = "Bitcoin"   // not Cash | Card | EFT | Voucher
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should().ThrowAsync<DomainException>();
        }

        [Fact]
        public async Task Handle_CancelledBooking_NeverCallsAddAsync()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Cancelled)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            var command = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 200m,
                PaymentMethod = "Cash"
            };

            // Act
            try { await _handler.Handle(command); } catch { /* expected */ }

            // Assert — no sale should be saved when booking is Cancelled
            _saleRepo.Verify(
                r => r.AddAsync(It.IsAny<Sale>()),
                Times.Never);
        }
    }
}
