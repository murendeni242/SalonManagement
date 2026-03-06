using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.GetAllCustomersHandlerTests.Success_Cases
{
    public class GetCustomerProfileHandlerTests
    {

        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly Mock<ISaleRepository> _saleRepo = new();
        private readonly GetCustomerProfileHandler _handler;

        public GetCustomerProfileHandlerTests()
        {
            _handler = new GetCustomerProfileHandler(
                _customerRepo.Object,
                _bookingRepo.Object,
                _saleRepo.Object);
        }

        [Fact]
        public async Task Handle_ExistingCustomer_ReturnsTotalSpentFromPaidSalesOnly()
        {
            // Arrange — one paid sale (R350) and one refunded sale (R200 negative)
            var customer = new CustomerBuilder().WithId(1).Build();
            var paid = new SaleBuilder().WithAmount(350m).WithStatus(SaleStatus.Paid).Build();
            var refunded = new SaleBuilder().WithAmount(200m).WithStatus(SaleStatus.Refunded).Build();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _bookingRepo.Setup(r => r.GetByCustomerIdAsync(1)).ReturnsAsync(new List<Booking>());
            _saleRepo.Setup(r => r.GetByCustomerIdAsync(1)).ReturnsAsync(new List<Sale> { paid, refunded });

            // Act
            var result = await _handler.Handle(1);

            // Assert — only Paid sales count toward TotalSpent
            result.TotalSpent.Should().Be(350m);
        }

        [Fact]
        public async Task Handle_ExistingCustomer_CountsOnlyCompletedBookingsAsVisits()
        {
            // Arrange — 2 completed, 1 pending
            var customer = new CustomerBuilder().WithId(1).Build();

            var bookings = new List<Booking>
        {
            new BookingBuilder().WithId(1).WithStatus(BookingStatus.Completed).Build(),
            new BookingBuilder().WithId(2).WithStatus(BookingStatus.Completed).Build(),
            new BookingBuilder().WithId(3).WithStatus(BookingStatus.Pending).Build(),
        };

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _bookingRepo.Setup(r => r.GetByCustomerIdAsync(1)).ReturnsAsync(bookings);
            _saleRepo.Setup(r => r.GetByCustomerIdAsync(1)).ReturnsAsync(new List<Sale>());

            // Act
            var result = await _handler.Handle(1);

            // Assert — only 2 completed bookings count as visits
            result.TotalVisits.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ExistingCustomer_ReturnsMaxFiveRecentBookings()
        {
            // Arrange — 7 bookings, should return latest 5 only
            var customer = new CustomerBuilder().WithId(1).Build();

            var bookings = Enumerable.Range(1, 7).Select(i =>
                new BookingBuilder().WithId(i).Build()).ToList();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _bookingRepo.Setup(r => r.GetByCustomerIdAsync(1)).ReturnsAsync(bookings);
            _saleRepo.Setup(r => r.GetByCustomerIdAsync(1)).ReturnsAsync(new List<Sale>());

            // Act
            var result = await _handler.Handle(1);

            // Assert
            result.RecentBookings.Should().HaveCount(5);
        }

        [Fact]
        public async Task Handle_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Customer?)null);

            // Act
            var act = () => _handler.Handle(99);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Customer*");
        }
    }
}
