using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.CreateCustomerHandlerTests.Failure_Cases
{
    public class CreateCustomerHandlerTests_FailureCases
    {
        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateCustomerHandler _handler;

        public CreateCustomerHandlerTests_FailureCases()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("reception@salon.co.za");

            _handler = new CreateCustomerHandler(
                _customerRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_DuplicatePhone_ThrowsDomainException()
        {
            // Arrange — phone already registered to another customer
            var existing = new CustomerBuilder()
                .WithPhone("0821234501")
                .Build();

            _customerRepo
                .Setup(r => r.GetByPhoneAsync("0821234501"))
                .ReturnsAsync(existing);

            var command = new CreateCustomerCommand
            {
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*0821234501*");
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ThrowsDomainException()
        {
            // Arrange — email already registered to another customer
            var existing = new CustomerBuilder()
                .WithEmail("taken@gmail.com")
                .Build();

            _customerRepo
                .Setup(r => r.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);  // phone is unique

            _customerRepo
                .Setup(r => r.GetByEmailAsync("taken@gmail.com"))
                .ReturnsAsync(existing);  // email is taken

            var command = new CreateCustomerCommand
            {
                FirstName = "Lindiwe",
                LastName = "Khumalo",
                Phone = "0821234999",
                Email = "taken@gmail.com"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*taken@gmail.com*");
        }

        [Fact]
        public async Task Handle_DuplicatePhone_NeverCallsAddAsync()
        {
            // Arrange
            var existing = new CustomerBuilder().WithPhone("0821234501").Build();

            _customerRepo
                .Setup(r => r.GetByPhoneAsync("0821234501"))
                .ReturnsAsync(existing);

            var command = new CreateCustomerCommand
            {
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501"
            };

            // Act
            try { await _handler.Handle(command); } catch { /* expected */ }

            // Assert — no customer should be saved when phone is duplicate
            _customerRepo.Verify(
                r => r.AddAsync(It.IsAny<Customer>()),
                Times.Never);
        }

        [Theory]
        [InlineData("", "Mokoena", "0821234501")]    // empty first name
        [InlineData("Zanele", "", "0821234501")]      // empty last name
        [InlineData("Zanele", "Mokoena", "")]         // empty phone
        public async Task Handle_MissingRequiredField_ThrowsDomainException(
            string firstName, string lastName, string phone)
        {
            // Arrange — Customer constructor validates required fields
            _customerRepo
                .Setup(r => r.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            var command = new CreateCustomerCommand
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — DomainException thrown by Customer constructor
            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
