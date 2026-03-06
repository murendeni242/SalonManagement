using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffManagement;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.StaffManagement.UpdateStaffHandlerTests.Failure_Cases
{
    public class UpdateStaffHandlerTests_FailureCases
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly UpdateStaffHandler _handler;

        public UpdateStaffHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new UpdateStaffHandler(
                _staffRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_StaffNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _staffRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Staff?)null);

            var command = new UpdateStaffCommand
            {
                Id = 99,
                FirstName = "X",
                LastName = "Y",
                Phone = "0712345603",
                Role = "Stylist",
                Status = "Active",
                Specialisations = new List<int>()
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Staff*");
        }
    }
}
