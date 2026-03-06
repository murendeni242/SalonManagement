using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Customers.CreateCustomerHandlerTests.Success_Cases
{
    public class CreateCustomerHandlerTests_HappyPath
    {
        // ── Shared mocks ───────────────────────────────────────────────
        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateCustomerHandler _handler;

        public CreateCustomerHandlerTests_HappyPath()
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
        public async Task Handle_ValidCommand_ReturnsCustomerDtoWithCorrectDetails()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetByPhoneAsync("0821234501"))
                .ReturnsAsync((Customer?)null);

            _customerRepo
                .Setup(r => r.GetByEmailAsync("zanele@gmail.com"))
                .ReturnsAsync((Customer?)null);

            _customerRepo
                .Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            var command = new CreateCustomerCommand
            {
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501",
                Email = "zanele@gmail.com",
                DateOfBirth = new DateTime(1990, 3, 15)
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.FirstName.Should().Be("Zanele");
            result.LastName.Should().Be("Mokoena");
            result.Phone.Should().Be("0821234501");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsync()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            _customerRepo
                .Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            var command = new CreateCustomerCommand
            {
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501"
            };

            // Act
            await _handler.Handle(command);

            // Assert — customer must be persisted
            _customerRepo.Verify(
                r => r.AddAsync(It.IsAny<Customer>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithCreatedAction()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            _customerRepo
                .Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            var command = new CreateCustomerCommand
            {
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501"
            };

            // Act
            await _handler.Handle(command);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Created" &&
                           log.EntityName == "Customer")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CommandWithoutEmail_SkipsEmailDuplicateCheck()
        {
            // Arrange — email is optional; when null, no email lookup should happen
            _customerRepo
                .Setup(r => r.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            _customerRepo
                .Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            var command = new CreateCustomerCommand
            {
                FirstName = "Kabelo",
                LastName = "Motsepe",
                Phone = "0821234504",
                Email = null   // ← no email
            };

            // Act
            await _handler.Handle(command);

            // Assert — GetByEmailAsync must never be called when email is null
            _customerRepo.Verify(
                r => r.GetByEmailAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsNotesWhenProvided()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);

            Customer? savedCustomer = null;

            _customerRepo
                .Setup(r => r.AddAsync(It.IsAny<Customer>()))
                .Callback<Customer>(c => savedCustomer = c)
                .Returns(Task.CompletedTask);

            var command = new CreateCustomerCommand
            {
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501",
                Notes = "Allergic to ammonia — use ammonia-free colour only."
            };

            // Act
            await _handler.Handle(command);

            // Assert
            savedCustomer.Should().NotBeNull();
            savedCustomer!.Notes.Should().Contain("ammonia");
        }
    }
}
