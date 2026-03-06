using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.GetAllCustomersHandlerTests.Success_Cases
{
    public class GetCustomerByIdHandlerTests
    {
        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly GetCustomerByIdHandler _handler;

        public GetCustomerByIdHandlerTests()
            => _handler = new GetCustomerByIdHandler(_customerRepo.Object);

        [Fact]
        public async Task Handle_ExistingCustomer_ReturnsMappedDto()
        {
            // Arrange
            var customer = new CustomerBuilder().WithId(1).Build();

            _customerRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(customer);

            // Act
            var result = await _handler.Handle(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CustomerNotFound_ReturnsNull()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _handler.Handle(99);

            // Assert — returns null, does NOT throw
            result.Should().BeNull();
        }
    }
}
