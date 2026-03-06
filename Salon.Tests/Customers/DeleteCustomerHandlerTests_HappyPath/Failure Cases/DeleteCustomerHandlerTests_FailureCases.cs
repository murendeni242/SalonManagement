using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Customers.DeleteCustomerHandlerTests_HappyPath.Failure_Cases
{
    public class DeleteCustomerHandlerTests_FailureCases
    {

        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteCustomerHandler _handler;

        public DeleteCustomerHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new DeleteCustomerHandler(
                _customerRepo.Object, _auditLog.Object, _currentUser.Object);
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
