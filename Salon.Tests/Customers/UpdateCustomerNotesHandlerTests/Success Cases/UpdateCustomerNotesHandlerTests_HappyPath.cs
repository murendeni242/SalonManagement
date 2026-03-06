using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.UpdateCustomerNotesHandlerTests.Success_Cases
{
    public class UpdateCustomerNotesHandlerTests_HappyPath
    {

        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly UpdateCustomerNotesHandler _handler;

        public UpdateCustomerNotesHandlerTests_HappyPath()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("reception@salon.co.za");
            _handler = new UpdateCustomerNotesHandler(
                _customerRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesNotesOnCustomer()
        {
            // Arrange
            var customer = new CustomerBuilder().WithId(1).Build();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _customerRepo.Setup(r => r.UpdateAsync(customer)).Returns(Task.CompletedTask);

            var command = new UpdateCustomerNotesCommand
            {
                Id = 1,
                Notes = "Prefers Nomsa as stylist. Allergic to ammonia."
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.Notes.Should().Contain("ammonia");
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithNotesUpdatedAction()
        {
            // Arrange
            var customer = new CustomerBuilder().WithId(1).Build();

            _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
            _customerRepo.Setup(r => r.UpdateAsync(customer)).Returns(Task.CompletedTask);

            var command = new UpdateCustomerNotesCommand { Id = 1, Notes = "VIP client." };

            // Act
            await _handler.Handle(command);

            // Assert — action must be "NotesUpdated" not "Updated"
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "NotesUpdated" &&
                           log.OldValues != null &&
                           log.NewValues != null)),
                Times.Once);
        }
    }
}
