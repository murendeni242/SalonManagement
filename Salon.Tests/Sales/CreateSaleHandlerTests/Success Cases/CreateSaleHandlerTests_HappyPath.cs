using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Sales;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Sales.CreateSaleHandlerTests.Success_Cases
{
    public class CreateSaleHandlerTests_HappyPath
    {
        // ── Shared mocks ───────────────────────────────────────────────
        private readonly Mock<ISaleRepository> _saleRepo = new();
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateSaleHandler _handler;

        public CreateSaleHandlerTests_HappyPath()
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
        public async Task Handle_ValidCommand_ReturnsSaleDtoWithCorrectAmount()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Completed)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            _saleRepo
                .Setup(r => r.AddAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var command = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 350m,
                PaymentMethod = "Card",
                Notes = null
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.BookingId.Should().Be(1);
            result.AmountPaid.Should().Be(350m);
            result.PaymentMethod.Should().Be("Card");
            result.Status.Should().Be("Paid");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsync()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Confirmed)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            _saleRepo
                .Setup(r => r.AddAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var command = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 250m,
                PaymentMethod = "Cash"
            };

            // Act
            await _handler.Handle(command);

            // Assert — sale must be persisted
            _saleRepo.Verify(
                r => r.AddAsync(It.Is<Sale>(s =>
                    s.BookingId == 1 &&
                    s.AmountPaid == 250m &&
                    s.PaymentMethod == "Cash")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLog()
        {
            // Arrange
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Completed)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            _saleRepo
                .Setup(r => r.AddAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var command = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 350m,
                PaymentMethod = "EFT"
            };

            // Act
            await _handler.Handle(command);

            // Assert — one audit entry with action = "Created"
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Created")),
                Times.Once);
        }

        [Theory]
        [InlineData(BookingStatus.Pending)]    // deposit before service
        [InlineData(BookingStatus.Confirmed)]  // deposit after confirmation
        [InlineData(BookingStatus.Completed)]  // full payment after service
        public async Task Handle_NonCancelledBooking_CreatesSaleSuccessfully(BookingStatus status)
        {
            // Arrange — payments are allowed on Pending, Confirmed, and Completed bookings
            //           only Cancelled is blocked
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(status)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            _saleRepo
                .Setup(r => r.AddAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var command = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 200m,
                PaymentMethod = "Cash"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — no exception for any non-Cancelled status
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_MultiplePaymentsSameBooking_BothSucceed()
        {
            // Arrange — deposit first, then balance — both must be accepted
            var booking = new BookingBuilder()
                .WithId(1)
                .WithStatus(BookingStatus.Pending)
                .Build();

            _bookingRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(booking);

            _saleRepo
                .Setup(r => r.AddAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var deposit = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 100m,
                PaymentMethod = "Card",
                Notes = "Deposit"
            };

            var balance = new CreateSaleCommand
            {
                BookingId = 1,
                AmountPaid = 250m,
                PaymentMethod = "Card",
                Notes = "Balance"
            };

            // Act — create both sales against the same booking
            var depositAct = () => _handler.Handle(deposit);
            var balanceAct = () => _handler.Handle(balance);

            // Assert — CreateSaleHandler does NOT block duplicate payments by design
            await depositAct.Should().NotThrowAsync();
            await balanceAct.Should().NotThrowAsync();
        }

    }
}
