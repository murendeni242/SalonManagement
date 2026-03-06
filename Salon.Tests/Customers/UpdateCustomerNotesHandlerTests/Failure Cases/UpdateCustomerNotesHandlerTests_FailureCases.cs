using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Customers.UpdateCustomerNotesHandlerTests.Failure_Cases
{
    public class UpdateCustomerNotesHandlerTests_FailureCases
    {
        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly UpdateCustomerNotesHandler _handler;

        public UpdateCustomerNotesHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("reception@salon.co.za");
            _handler = new UpdateCustomerNotesHandler(
                _customerRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Customer?)null);

            var command = new UpdateCustomerNotesCommand { Id = 99, Notes = "Test" };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Customer*");
        }
    }
}
