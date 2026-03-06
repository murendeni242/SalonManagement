using FluentAssertions;
using Moq;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.StaffManagement.GetStaffHandlerTests.Success_Cases
{
    public class GetStaffByIdHandlerTests
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly GetStaffByIdHandler _handler;

        public GetStaffByIdHandlerTests()
            => _handler = new GetStaffByIdHandler(_staffRepo.Object);

        [Fact]
        public async Task Handle_ExistingStaff_ReturnsMappedDto()
        {
            // Arrange
            var staff = new StaffBuilder().WithId(1).WithFirstName("Nomsa").Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);

            // Act
            var result = await _handler.Handle(1);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Nomsa");
        }

        [Fact]
        public async Task Handle_StaffNotFound_ReturnsNull()
        {
            // Arrange
            _staffRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Staff?)null);

            // Act
            var result = await _handler.Handle(99);

            // Assert — returns null, does NOT throw
            result.Should().BeNull();
        }
    }
}
