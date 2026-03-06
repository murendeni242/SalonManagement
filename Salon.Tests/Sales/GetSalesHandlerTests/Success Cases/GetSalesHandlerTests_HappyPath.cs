using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Sales;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Sales.GetSalesHandlerTests.Success_Cases
{
    public class GetSalesHandlerTests_HappyPath
    {
        private readonly Mock<ISaleRepository> _saleRepo = new();
        private readonly GetSalesHandler _handler;

        public GetSalesHandlerTests_HappyPath()
        {
            _handler = new GetSalesHandler(_saleRepo.Object);
        }

        [Fact]
        public async Task Handle_WithSales_ReturnsSaleDtos()
        {
            // Arrange — two paid sales in the repository
            var sales = new List<Sale>
        {
            new SaleBuilder().WithId(1).WithAmount(300m).WithStatus(SaleStatus.Paid).Build(),
            new SaleBuilder().WithId(2).WithAmount(500m).WithStatus(SaleStatus.Paid).Build(),
        };

            _saleRepo
                .Setup(r => r.GetPagedAsync(null, null, 0, 50))
                .ReturnsAsync(sales);

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Sales.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithPaidSales_CalculatesTotalRevenue()
        {
            // Arrange — R300 + R500 = R800 total revenue
            var sales = new List<Sale>
        {
            new SaleBuilder().WithId(1).WithAmount(300m).WithStatus(SaleStatus.Paid).Build(),
            new SaleBuilder().WithId(2).WithAmount(500m).WithStatus(SaleStatus.Paid).Build(),
        };

            _saleRepo
                .Setup(r => r.GetPagedAsync(null, null, 0, 50))
                .ReturnsAsync(sales);

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Summary.TotalRevenue.Should().Be(800m);
            result.Summary.TotalTransactions.Should().Be(2);
            result.Summary.NetRevenue.Should().Be(800m);
        }

        [Fact]
        public async Task Handle_WithRefund_DeductsFromNetRevenue()
        {
            // Arrange — R500 paid, R200 refunded → net R300
            var paid = new SaleBuilder()
                .WithId(1).WithAmount(500m).WithStatus(SaleStatus.Paid).Build();

            // Build refund sale manually — negative amount, Refunded status
            var refund = new SaleBuilder()
                .WithId(2).WithAmount(200m).WithStatus(SaleStatus.Refunded).Build();

            // Force AmountPaid to negative to simulate what Sale.Refund() produces
            typeof(Sale)
                .GetProperty("AmountPaid",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance)!
                .SetValue(refund, -200m);

            var sales = new List<Sale> { paid, refund };

            _saleRepo
                .Setup(r => r.GetPagedAsync(null, null, 0, 50))
                .ReturnsAsync(sales);

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Summary.TotalRevenue.Should().Be(500m);
            result.Summary.TotalRefunded.Should().Be(200m);
            result.Summary.NetRevenue.Should().Be(300m);
        }

        [Fact]
        public async Task Handle_EmptyRepository_ReturnsZeroSummary()
        {
            // Arrange — no sales in the system yet
            _saleRepo
                .Setup(r => r.GetPagedAsync(null, null, 0, 50))
                .ReturnsAsync(new List<Sale>());

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Sales.Should().BeEmpty();
            result.Summary.TotalRevenue.Should().Be(0m);
            result.Summary.TotalRefunded.Should().Be(0m);
            result.Summary.NetRevenue.Should().Be(0m);
            result.Summary.TotalTransactions.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithDateRange_PassesFiltersToRepository()
        {
            // Arrange
            var from = new DateTime(2025, 1, 1);
            var to = new DateTime(2025, 1, 31);

            _saleRepo
                .Setup(r => r.GetPagedAsync(from, to, 0, 50))
                .ReturnsAsync(new List<Sale>());

            // Act
            await _handler.Handle(from: from, to: to);

            // Assert — filters must be forwarded to the repository exactly
            _saleRepo.Verify(
                r => r.GetPagedAsync(from, to, 0, 50),
                Times.Once);
        }
    }
}
