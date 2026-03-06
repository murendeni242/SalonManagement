using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffManagement;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.StaffManagement.DeleteStaffHandlerTests.Failure_Cases
{
    public class DeleteStaffHandlerTests_FailureCases
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteStaffHandler _handler;

        public DeleteStaffHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new DeleteStaffHandler(
                _staffRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_StaffNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _staffRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Staff?)null);

            // Act
            var act = () => _handler.Handle(99);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("*Staff*");
        }
    }
}
