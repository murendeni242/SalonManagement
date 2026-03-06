using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Services;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Services.GetServicesHandlerTests.Success_Cases
{
    public class GetServicesHandlerTests
    {
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly GetServicesHandler _handler;

        public GetServicesHandlerTests()
            => _handler = new GetServicesHandler(_serviceRepo.Object);

        [Fact]
        public async Task Handle_WithServices_ReturnsMappedDtos()
        {
            // Arrange
            var services = new List<Service>
        {
            new ServiceBuilder().WithId(1).WithName("Wash & Blow Dry").Build(),
            new ServiceBuilder().WithId(2).WithName("Full Colour").Build(),
        };

            _serviceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(services);

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Should().HaveCount(2);
            result.Select(s => s.Name).Should().Contain("Wash & Blow Dry");
        }

        [Fact]
        public async Task Handle_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _serviceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Service>());

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Should().BeEmpty();
        }
    }
}
