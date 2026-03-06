using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.UpdateCustomerHandlerTests.Success_Cases
{
    public class UpdateCustomerHandlerTests_HappyPath
    {

        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly UpdateCustomerHandler _handler;

        public UpdateCustomerHandlerTests_HappyPath()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("reception@salon.co.za");

            _handler = new UpdateCustomerHandler(
                _customerRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedDto()
        {
            // Arrange
            var customer = new CustomerBuilder()
                .WithId(1)
                .WithPhone("0821234501")
                .WithEmail("old@gmail.com")
                .Build();

            _customerRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(customer);

            _customerRepo
                .Setup(r => r.GetByPhoneAsync("0821234502"))
                .ReturnsAsync((Customer?)null);  // new phone is available

            _customerRepo
                .Setup(r => r.GetByEmailAsync("new@gmail.com"))
                .ReturnsAsync((Customer?)null);  // new email is available

            _customerRepo
                .Setup(r => r.UpdateAsync(customer))
                .Returns(Task.CompletedTask);

            var command = new UpdateCustomerCommand
            {
                Id = 1,
                FirstName = "Zanele",
                LastName = "Dlamini",   // ← changed
                Phone = "0821234502",
                Email = "new@gmail.com"
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.LastName.Should().Be("Dlamini");
            result.Phone.Should().Be("0821234502");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync()
        {
            // Arrange
            var customer = new CustomerBuilder().WithId(1).WithPhone("0821234501").Build();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _customerRepo.Setup(r => r.GetByPhoneAsync("0821234501")).ReturnsAsync((Customer?)null);
            _customerRepo.Setup(r => r.UpdateAsync(customer)).Returns(Task.CompletedTask);

            var command = new UpdateCustomerCommand
            {
                Id = 1,
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501"
            };

            // Act
            await _handler.Handle(command);

            // Assert
            _customerRepo.Verify(r => r.UpdateAsync(customer), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithOldAndNewSnapshots()
        {
            // Arrange
            var customer = new CustomerBuilder().WithId(1).WithPhone("0821234501").Build();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _customerRepo.Setup(r => r.GetByPhoneAsync(It.IsAny<string>())).ReturnsAsync((Customer?)null);
            _customerRepo.Setup(r => r.UpdateAsync(customer)).Returns(Task.CompletedTask);

            var command = new UpdateCustomerCommand
            {
                Id = 1,
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501"
            };

            // Act
            await _handler.Handle(command);

            // Assert — audit log must contain both old and new snapshots
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Updated" &&
                           log.OldValues != null &&
                           log.NewValues != null)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SamePhoneAsOwn_SkipsPhoneUniquenessCheck()
        {
            // Arrange — customer keeps their existing phone number; no lookup needed
            var customer = new CustomerBuilder()
                .WithId(1)
                .WithPhone("0821234501")
                .Build();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _customerRepo.Setup(r => r.UpdateAsync(customer)).Returns(Task.CompletedTask);

            var command = new UpdateCustomerCommand
            {
                Id = 1,
                FirstName = "Zanele",
                LastName = "Mokoena",
                Phone = "0821234501"   // ← same phone, no change
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — no exception, phone uniqueness check skipped
            await act.Should().NotThrowAsync();
        }
    }
}
