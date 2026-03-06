using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.GetAllCustomersHandlerTests.Success_Cases
{
    public class GetAllCustomersHandlerTests
    {
        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly GetAllCustomersHandler _handler;

        public GetAllCustomersHandlerTests()
            => _handler = new GetAllCustomersHandler(_customerRepo.Object);

        [Fact]
        public async Task Handle_WithCustomers_ReturnsMappedDtos()
        {
            // Arrange
            var customers = new List<Customer>
        {
            new CustomerBuilder().WithId(1).WithLastName("Khumalo").Build(),
            new CustomerBuilder().WithId(2).WithLastName("Mokoena").Build(),
        };

            _customerRepo
                .Setup(r => r.GetPagedAsync(0, 50))
                .ReturnsAsync(customers);

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetPagedAsync(0, 50))
                .ReturnsAsync(new List<Customer>());

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PassesPaginationToRepository()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.GetPagedAsync(10, 20))
                .ReturnsAsync(new List<Customer>());

            // Act
            await _handler.Handle(skip: 10, take: 20);

            // Assert — pagination values must be forwarded exactly
            _customerRepo.Verify(r => r.GetPagedAsync(10, 20), Times.Once);
        }
    }
}
