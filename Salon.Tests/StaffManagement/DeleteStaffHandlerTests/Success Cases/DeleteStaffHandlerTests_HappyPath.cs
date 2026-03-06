using Moq;
using Salon.Application.UseCases.StaffManagement;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.StaffManagement.DeleteStaffHandlerTests.Success_Cases
{
    public class DeleteStaffHandlerTests_HappyPath
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteStaffHandler _handler;

        public DeleteStaffHandlerTests_HappyPath()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new DeleteStaffHandler(
                _staffRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ExistingStaff_CallsDeleteAsync()
        {
            // Arrange
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.DeleteAsync(staff)).Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(1);

            // Assert — soft delete goes through DeleteAsync
            _staffRepo.Verify(r => r.DeleteAsync(staff), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingStaff_WritesAuditLogWithSoftDeletedAction()
        {
            // Arrange
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.DeleteAsync(staff)).Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(1);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "SoftDeleted" &&
                           log.EntityName == "Staff")),
                Times.Once);
        }
    }
}
