using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Services;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Services.GetServicesHandlerTests.Success_Cases
{
    public class GetServiceByIdHandlerTests
    {
        private readonly Mock<IServiceRepository> _serviceRepo = new();
        private readonly GetServiceByIdHandler _handler;

        public GetServiceByIdHandlerTests()
            => _handler = new GetServiceByIdHandler(_serviceRepo.Object);

        [Fact]
        public async Task Handle_ExistingService_ReturnsMappedDto()
        {
            // Arrange
            var service = new ServiceBuilder().WithId(1).WithName("Highlights").Build();

            _serviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(service);

            // Act
            var result = await _handler.Handle(1);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Highlights");
        }

        [Fact]
        public async Task Handle_ServiceNotFound_ReturnsNull()
        {
            // Arrange
            _serviceRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Service?)null);

            // Act
            var result = await _handler.Handle(99);

            // Assert — returns null, does NOT throw
            result.Should().BeNull();
        }
    }
}
