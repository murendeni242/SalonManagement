using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.UpdateCustomerHandlerTests.Failure_Cases
{
    public class UpdateCustomerHandlerTests_FailureCases
    {

        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly UpdateCustomerHandler _handler;

        public UpdateCustomerHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("reception@salon.co.za");
            _handler = new UpdateCustomerHandler(
                _customerRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Customer?)null);

            var command = new UpdateCustomerCommand
            {
                Id = 99,
                FirstName = "X",
                LastName = "Y",
                Phone = "0821234501"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Customer*");
        }

        [Fact]
        public async Task Handle_PhoneTakenByDifferentCustomer_ThrowsDomainException()
        {
            // Arrange — the new phone already belongs to customer Id=2
            var customer = new CustomerBuilder().WithId(1).WithPhone("0821234501").Build();
            var other = new CustomerBuilder().WithId(2).WithPhone("0821234502").Build();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _customerRepo.Setup(r => r.GetByPhoneAsync("0821234502")).ReturnsAsync(other);

            var command = new UpdateCustomerCommand
            {
                Id = 1,
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234502"   // ← belongs to customer Id=2
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already registered*");
        }

        [Fact]
        public async Task Handle_EmailTakenByDifferentCustomer_ThrowsDomainException()
        {
            // Arrange
            var customer = new CustomerBuilder().WithId(1).WithPhone("0821234501").WithEmail("mine@gmail.com").Build();
            var other = new CustomerBuilder().WithId(2).WithPhone("0821234502").WithEmail("taken@gmail.com").Build();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _customerRepo.Setup(r => r.GetByPhoneAsync("0821234501")).ReturnsAsync((Customer?)null);
            _customerRepo.Setup(r => r.GetByEmailAsync("taken@gmail.com")).ReturnsAsync(other);

            var command = new UpdateCustomerCommand
            {
                Id = 1,
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501",
                Email = "taken@gmail.com"   // ← belongs to customer Id=2
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*already registered*");
        }
    }
}
