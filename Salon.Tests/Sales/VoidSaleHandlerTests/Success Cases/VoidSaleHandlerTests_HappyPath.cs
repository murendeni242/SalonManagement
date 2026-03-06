using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Sales;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Sales.VoidSaleHandlerTests.Success_Cases
{
    public class VoidSaleHandlerTests_HappyPath
    {
        private readonly Mock<ISaleRepository> _saleRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly VoidSaleHandler _handler;

        public VoidSaleHandlerTests_HappyPath()
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
        public async Task Handle_PaidSale_SetsStatusToVoided()
        {
            // Arrange
            var sale = new SaleBuilder()
                .WithId(1)
                .WithAmount(300m)
                .WithStatus(SaleStatus.Paid)
                .Build();

            _saleRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(sale);

            _saleRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var command = new VoidSaleCommand
            {
                SaleId = 1,
                Reason = "Wrong amount entered by mistake."
            };

            // Act
            await _handler.Handle(command);

            // Assert — domain method Void() changes status to Voided
            sale.Status.Should().Be(SaleStatus.Voided);
        }

        [Fact]
        public async Task Handle_PaidSale_CallsUpdateAsync()
        {
            // Arrange — record stays in DB with Status = Voided (not deleted)
            var sale = new SaleBuilder()
                .WithId(1)
                .WithStatus(SaleStatus.Paid)
                .Build();

            _saleRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(sale);

            _saleRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var command = new VoidSaleCommand
            {
                SaleId = 1,
                Reason = "Linked to wrong booking."
            };

            // Act
            await _handler.Handle(command);

            // Assert — must call UpdateAsync, never DeleteAsync
            _saleRepo.Verify(r => r.UpdateAsync(sale), Times.Once);
        }

        [Fact]
        public async Task Handle_PaidSale_WritesAuditLogWithVoidedAction()
        {
            // Arrange
            var sale = new SaleBuilder()
                .WithId(1)
                .WithStatus(SaleStatus.Paid)
                .Build();

            _saleRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(sale);

            _saleRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var command = new VoidSaleCommand
            {
                SaleId = 1,
                Reason = "Data entry error."
            };

            // Act
            await _handler.Handle(command);

            // Assert — audit log must record both old and new snapshots
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Voided" &&
                           log.OldValues != null &&
                           log.NewValues != null)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PaidSale_OnlyCallsUpdateAsyncOnce()
        {
            // Arrange — void updates the record in place, must not call save more than once
            var sale = new SaleBuilder()
                .WithId(1)
                .WithStatus(SaleStatus.Paid)
                .Build();

            _saleRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(sale);

            _saleRepo
                .Setup(r => r.UpdateAsync(It.IsAny<Sale>()))
                .Returns(Task.CompletedTask);

            var command = new VoidSaleCommand
            {
                SaleId = 1,
                Reason = "Error."
            };

            // Act
            await _handler.Handle(command);

            // Assert — UpdateAsync called exactly once (not twice, not zero)
            _saleRepo.Verify(
                r => r.UpdateAsync(sale),
                Times.Once);
        }
    }
}
