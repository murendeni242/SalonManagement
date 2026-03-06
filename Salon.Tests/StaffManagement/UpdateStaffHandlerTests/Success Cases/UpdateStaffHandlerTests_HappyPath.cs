using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffManagement;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.StaffManagement.UpdateStaffHandlerTests.Success_Cases
{
    public class UpdateStaffHandlerTests_HappyPath
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly UpdateStaffHandler _handler;

        public UpdateStaffHandlerTests_HappyPath()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new UpdateStaffHandler(
                _staffRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedDto()
        {
            // Arrange
            var staff = new StaffBuilder()
                .WithId(1)
                .WithFirstName("Nomsa")
                .WithRole("Stylist")
                .Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.UpdateAsync(staff)).Returns(Task.CompletedTask);

            var command = new UpdateStaffCommand
            {
                Id = 1,
                FirstName = "Nomsa",
                LastName = "Dlamini",       
                Phone = "0712345699",
                Role = "Senior Stylist",
                Status = "Active",
                Specialisations = new List<int> { 1, 2, 3 }
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.LastName.Should().Be("Dlamini");
            result.Role.Should().Be("Senior Stylist");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync()
        {
            // Arrange
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.UpdateAsync(staff)).Returns(Task.CompletedTask);

            var command = new UpdateStaffCommand
            {
                Id = 1,
                FirstName = "Nomsa",
                LastName = "Zulu",
                Phone = "0712345603",
                Role = "Stylist",
                Status = "Active",
                Specialisations = new List<int>()
            };

            // Act
            await _handler.Handle(command);

            // Assert
            _staffRepo.Verify(r => r.UpdateAsync(staff), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithOldAndNewSnapshots()
        {
            // Arrange
            var staff = new StaffBuilder().WithId(1).Build();

            _staffRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
            _staffRepo.Setup(r => r.UpdateAsync(staff)).Returns(Task.CompletedTask);

            var command = new UpdateStaffCommand
            {
                Id = 1,
                FirstName = "Nomsa",
                LastName = "Zulu",
                Phone = "0712345603",
                Role = "Stylist",
                Status = "Active",
                Specialisations = new List<int>()
            };

            // Act
            await _handler.Handle(command);

            // Assert — both snapshots must be present in the audit entry
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Updated" &&
                           log.OldValues != null &&
                           log.NewValues != null)),
                Times.Once);
        }
    }
}
