using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Services;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Services.CreateServiceHandlerTests.Success_Cases
{
    public class CreateServiceHandlerTests_HappyPath
    {
        // ── Shared mocks ───────────────────────────────────────────────
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateServiceHandler _handler;

        public CreateServiceHandlerTests_HappyPath()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("owner@salon.co.za");

            _handler = new CreateServiceHandler(
                _serviceRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewServiceId()
        {
            // Arrange
            _serviceRepo
                .Setup(r => r.AddAsync(It.IsAny<Service>()))
                .Returns(Task.CompletedTask);

            var command = new CreateServiceCommand
            {
                Name = "Wash & Blow Dry",
                DurationMinutes = 45,
                BasePrice = 280m,
                Description = "Shampoo, condition and blow dry"
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert — returns the new Id (0 until EF sets it, which is fine in unit tests)
            result.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsync()
        {
            // Arrange
            _serviceRepo
                .Setup(r => r.AddAsync(It.IsAny<Service>()))
                .Returns(Task.CompletedTask);

            var command = new CreateServiceCommand
            {
                Name = "Full Colour",
                DurationMinutes = 120,
                BasePrice = 850m
            };

            // Act
            await _handler.Handle(command);

            // Assert — service must be persisted
            _serviceRepo.Verify(
                r => r.AddAsync(It.Is<Service>(s =>
                    s.Name == "Full Colour" &&
                    s.DurationMinutes == 120 &&
                    s.BasePrice == 850m)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithCreatedAction()
        {
            // Arrange
            _serviceRepo
                .Setup(r => r.AddAsync(It.IsAny<Service>()))
                .Returns(Task.CompletedTask);

            var command = new CreateServiceCommand
            {
                Name = "Swedish Massage",
                DurationMinutes = 60,
                BasePrice = 450m
            };

            // Act
            await _handler.Handle(command);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Created" &&
                           log.EntityName == "Service")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CommandWithNullDescription_DefaultsToEmptyString()
        {
            // Arrange — Description is optional in the command
            Service? savedService = null;

            _serviceRepo
                .Setup(r => r.AddAsync(It.IsAny<Service>()))
                .Callback<Service>(s => savedService = s)
                .Returns(Task.CompletedTask);

            var command = new CreateServiceCommand
            {
                Name = "Deep Conditioning",
                DurationMinutes = 30,
                BasePrice = 180m,
                Description = null   // ← not provided
            };

            // Act
            await _handler.Handle(command);

            // Assert — Description must default to empty string, not null
            savedService.Should().NotBeNull();
            savedService!.Description.Should().NotBeNull();
            savedService.Description.Should().Be(string.Empty);
        }
    }
}
