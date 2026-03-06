using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Services;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Services.UpdateServiceHandlerTests.Success_Cases
{
    public class UpdateServiceHandlerTests_HappyPath
    {
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly UpdateServiceHandler _handler;

        public UpdateServiceHandlerTests_HappyPath()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new UpdateServiceHandler(
                _serviceRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedDto()
        {
            // Arrange
            var service = new ServiceBuilder().WithId(1).WithName("Old Name").WithBasePrice(200m).Build();

            _serviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);
            _serviceRepo.Setup(r => r.UpdateAsync(service)).Returns(Task.CompletedTask);

            var command = new UpdateServiceCommand
            {
                Id = 1,
                Name = "Wash & Style",   // ← changed
                DurationMinutes = 60,
                BasePrice = 350m,             // ← changed
                Description = "Updated desc",
                Status = service.Status
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.Name.Should().Be("Wash & Style");
            result.BasePrice.Should().Be(350m);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync()
        {
            // Arrange
            var service = new ServiceBuilder().WithId(1).Build();

            _serviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);
            _serviceRepo.Setup(r => r.UpdateAsync(service)).Returns(Task.CompletedTask);

            var command = new UpdateServiceCommand
            {
                Id = 1,
                Name = "Updated Name",
                DurationMinutes = 45,
                BasePrice = 280m,
                Status = service.Status
            };

            // Act
            await _handler.Handle(command);

            // Assert
            _serviceRepo.Verify(r => r.UpdateAsync(service), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithOldAndNewSnapshots()
        {
            // Arrange
            var service = new ServiceBuilder().WithId(1).Build();

            _serviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);
            _serviceRepo.Setup(r => r.UpdateAsync(service)).Returns(Task.CompletedTask);

            var command = new UpdateServiceCommand
            {
                Id = 1,
                Name = "Updated Name",
                DurationMinutes = 45,
                BasePrice = 280m,
                Status = service.Status
            };

            // Act
            await _handler.Handle(command);

            // Assert — both old and new snapshots must be in the audit log
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Updated" &&
                           log.OldValues != null &&
                           log.NewValues != null)),
                Times.Once);
        }
    }
}
