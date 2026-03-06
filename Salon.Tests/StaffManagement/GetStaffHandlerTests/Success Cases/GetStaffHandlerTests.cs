using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffManagement;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.StaffManagement.GetStaffHandlerTests.Success_Cases
{
    public class GetStaffHandlerTests
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly GetStaffHandler _handler;

        public GetStaffHandlerTests()
            => _handler = new GetStaffHandler(_staffRepo.Object);

        [Fact]
        public async Task Handle_WithStaff_ReturnsMappedDtos()
        {
            // Arrange
            var staffList = new List<Staff>
        {
            new StaffBuilder().WithId(1).WithLastName("Zulu").Build(),
            new StaffBuilder().WithId(2).WithLastName("Mahlangu").Build(),
        };

            _staffRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(staffList);

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Should().HaveCount(2);
            result.Select(s => s.LastName).Should().Contain("Zulu");
        }

        [Fact]
        public async Task Handle_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _staffRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Staff>());

            // Act
            var result = await _handler.Handle();

            // Assert
            result.Should().BeEmpty();
        }
    }
}
