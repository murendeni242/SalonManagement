using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Services;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Services.CreateServiceHandlerTests.Failure_Cases
{
    public class CreateServiceHandlerTests_FailureCases
    {
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateServiceHandler _handler;

        public CreateServiceHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new CreateServiceHandler(
                _serviceRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_EmptyName_ThrowsDomainException()
        {
            // Arrange — Service constructor validates name
            var command = new CreateServiceCommand
            {
                Name = "",
                DurationMinutes = 45,
                BasePrice = 280m
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*name*");
        }

        [Theory]
        [InlineData(0)]    // zero duration not allowed
        [InlineData(-10)]  // negative duration not allowed
        public async Task Handle_InvalidDuration_ThrowsDomainException(int duration)
        {
            // Arrange
            var command = new CreateServiceCommand
            {
                Name = "Valid Service",
                DurationMinutes = duration,
                BasePrice = 280m
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*Duration*");
        }

        [Fact]
        public async Task Handle_NegativeBasePrice_ThrowsDomainException()
        {
            // Arrange
            var command = new CreateServiceCommand
            {
                Name = "Valid Service",
                DurationMinutes = 45,
                BasePrice = -1m   // ← invalid
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("*price*");
        }

        [Fact]
        public async Task Handle_ZeroBasePrice_DoesNotThrow()
        {
            // Arrange — R0 is valid (e.g. complimentary services)
            _serviceRepo
                .Setup(r => r.AddAsync(It.IsAny<Service>()))
                .Returns(Task.CompletedTask);

            var command = new CreateServiceCommand
            {
                Name = "Complimentary Consultation",
                DurationMinutes = 15,
                BasePrice = 0m   // ← valid
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — R0 is allowed by domain rule (>= 0, not > 0)
            await act.Should().NotThrowAsync();
        }
    }
}
