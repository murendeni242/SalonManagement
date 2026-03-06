using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.DeleteCustomerHandlerTests_HappyPath.Success_Cases
{
    public class DeleteCustomerHandlerTests_HappyPath
    {

        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteCustomerHandler _handler;

        public DeleteCustomerHandlerTests_HappyPath()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("owner@salon.co.za");

            _handler = new DeleteCustomerHandler(
                _customerRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ExistingCustomer_CallsDeleteAsync()
        {
            // Arrange
            var customer = new CustomerBuilder().WithId(1).Build();

            _customerRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(customer);

            _customerRepo
                .Setup(r => r.DeleteAsync(customer))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(1);

            // Assert — DeleteAsync triggers soft delete via DbContext interceptor
            _customerRepo.Verify(r => r.DeleteAsync(customer), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingCustomer_WritesAuditLogWithSoftDeletedAction()
        {
            // Arrange
            var customer = new CustomerBuilder().WithId(1).Build();

            _customerRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(customer);

            _customerRepo
                .Setup(r => r.DeleteAsync(customer))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(1);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "SoftDeleted" &&
                           log.EntityName == "Customer")),
                Times.Once);
        }
    }
}
