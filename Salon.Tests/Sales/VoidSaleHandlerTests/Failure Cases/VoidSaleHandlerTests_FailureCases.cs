using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Sales;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Sales.VoidSaleHandlerTests.Failure_Cases
{
    public class VoidSaleHandlerTests_FailureCases
    {
        private readonly Mock<ISaleRepository> _saleRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly VoidSaleHandler _handler;

        public VoidSaleHandlerTests_FailureCases()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("owner@salon.co.za");

            _handler = new VoidSaleHandler(
                _saleRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_SaleNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _saleRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Sale?)null);

            var command = new VoidSaleCommand
            {
                SaleId = 99,
                Reason = "Error."
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Sale*");
        }

        [Theory]
        [InlineData(SaleStatus.Refunded)]  // already refunded — cannot void
        [InlineData(SaleStatus.Voided)]    // already voided — cannot void twice
        public async Task Handle_NonPaidSale_ThrowsDomainException(SaleStatus status)
        {
            // Arrange — only Paid sales can be voided
            var sale = new SaleBuilder()
                .WithId(1)
                .WithStatus(status)
                .Build();

            _saleRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(sale);

            var command = new VoidSaleCommand
            {
                SaleId = 1,
                Reason = "Mistake."
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — domain rule: "Only Paid sales can be voided"
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*Paid*");
        }

        [Fact]
        public async Task Handle_EmptyReason_ThrowsDomainException()
        {
            // Arrange — reason is mandatory on a void (audit requirement)
            var sale = new SaleBuilder()
                .WithId(1)
                .WithStatus(SaleStatus.Paid)
                .Build();

            _saleRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(sale);

            var command = new VoidSaleCommand
            {
                SaleId = 1,
                Reason = ""   // ← blank reason
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*reason*");
        }
    }
}
