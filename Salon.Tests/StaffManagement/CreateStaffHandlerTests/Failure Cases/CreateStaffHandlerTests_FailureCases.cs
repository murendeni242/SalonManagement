using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffManagement;
using Salon.Domain.Common;
using Salon.Domain.Interfaces;

namespace Salon.Tests.StaffManagement.CreateStaffHandlerTests.Failure_Cases
{
    public class CreateStaffHandlerTests_FailureCases
    {
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateStaffHandler _handler;

        public CreateStaffHandlerTests_FailureCases()
        {
            _currentUser.Setup(x => x.UserEmail).Returns("owner@salon.co.za");
            _handler = new CreateStaffHandler(
                _staffRepo.Object, _auditLog.Object, _currentUser.Object);
        }

        [Theory]
        [InlineData("", "Zulu", "0712345603", "Stylist")]      // empty first name
        [InlineData("Nomsa", "", "0712345603", "Stylist")]      // empty last name
        [InlineData("Nomsa", "Zulu", "0712345603", "")]         // empty role
        public async Task Handle_MissingRequiredField_ThrowsDomainException(
            string firstName, string lastName, string phone, string role)
        {
            // Arrange — Staff constructor validates required fields
            var command = new CreateStaffCommand
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Role = role,
                Specialisations = new List<int>()
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — DomainException thrown by Staff constructor
            await act.Should().ThrowAsync<DomainException>();
        }
    }
}
