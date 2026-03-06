using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Customers;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Customers.GetAllCustomersHandlerTests.Success_Cases
{
    public class SearchCustomersHandlerTests
    {
        private readonly Mock<ICustomerRepository> _customerRepo = new();
        private readonly SearchCustomersHandler _handler;

        public SearchCustomersHandlerTests()
            => _handler = new SearchCustomersHandler(_customerRepo.Object);

        [Fact]
        public async Task Handle_ValidSearchTerm_ReturnsMatchingDtos()
        {
            // Arrange
            var matches = new List<Customer>
        {
            new CustomerBuilder().WithId(1).WithLastName("Mokoena").Build()
        };

            _customerRepo
                .Setup(r => r.SearchAsync("Mokoena"))
                .ReturnsAsync(matches);

            // Act
            var result = await _handler.Handle("Mokoena");

            // Assert
            result.Should().HaveCount(1);
            result.First().LastName.Should().Be("Mokoena");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Handle_EmptyOrWhitespaceSearchTerm_ReturnsEmptyWithoutCallingRepo(
            string? searchTerm)
        {
            // Arrange — blank search terms return empty immediately

            // Act
            var result = await _handler.Handle(searchTerm!);

            // Assert
            result.Should().BeEmpty();

            // SearchAsync must never be called — no point hitting the database
            _customerRepo.Verify(
                r => r.SearchAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_SearchTerm_TrimsWhitespaceBeforeQuery()
        {
            // Arrange
            _customerRepo
                .Setup(r => r.SearchAsync("Zanele"))
                .ReturnsAsync(new List<Customer>());

            // Act
            await _handler.Handle("  Zanele  ");

            // Assert — must be trimmed before passing to repository
            _customerRepo.Verify(r => r.SearchAsync("Zanele"), Times.Once);
        }
    }
}
