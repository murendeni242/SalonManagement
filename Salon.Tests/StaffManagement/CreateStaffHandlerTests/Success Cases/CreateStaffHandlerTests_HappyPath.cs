using FluentAssertions;
using Moq;
using Salon.Application.UseCases.StaffManagement;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.StaffManagement.CreateStaffHandlerTests.Success_Cases
{
    public class CreateStaffHandlerTests_HappyPath
    {
        // ── Shared mocks ───────────────────────────────────────────────
        private readonly Mock<IStaffRepository> _staffRepo = new();
        private readonly Mock<IAuditLogRepository> _auditLog = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly CreateStaffHandler _handler;

        public CreateStaffHandlerTests_HappyPath()
        {
            _currentUser
                .Setup(x => x.UserEmail)
                .Returns("owner@salon.co.za");

            _handler = new CreateStaffHandler(
                _staffRepo.Object,
                _auditLog.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsStaffDtoWithCorrectDetails()
        {
            // Arrange
            _staffRepo
                .Setup(r => r.AddAsync(It.IsAny<Staff>()))
                .Returns(Task.CompletedTask);

            var command = new CreateStaffCommand
            {
                FirstName = "Nomsa",
                LastName = "Zulu",
                Phone = "0712345603",
                Role = "Stylist",
                Email = "nomsa.zulu@salon.co.za",
                Specialisations = new List<int>()
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.FirstName.Should().Be("Nomsa");
            result.LastName.Should().Be("Zulu");
            result.Role.Should().Be("Stylist");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsync()
        {
            // Arrange
            _staffRepo
                .Setup(r => r.AddAsync(It.IsAny<Staff>()))
                .Returns(Task.CompletedTask);

            var command = new CreateStaffCommand
            {
                FirstName = "Sipho",
                LastName = "Mahlangu",
                Phone = "0712345604",
                Role = "Colourist",
                Specialisations = new List<int>()
            };

            // Act
            await _handler.Handle(command);

            // Assert — staff profile must be persisted
            _staffRepo.Verify(
                r => r.AddAsync(It.Is<Staff>(s =>
                    s.FirstName == "Sipho" &&
                    s.LastName == "Mahlangu" &&
                    s.Role == "Colourist")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_WritesAuditLogWithCreatedAction()
        {
            // Arrange
            _staffRepo
                .Setup(r => r.AddAsync(It.IsAny<Staff>()))
                .Returns(Task.CompletedTask);

            var command = new CreateStaffCommand
            {
                FirstName = "Nomsa",
                LastName = "Zulu",
                Phone = "0712345603",
                Role = "Stylist",
                Specialisations = new List<int>()
            };

            // Act
            await _handler.Handle(command);

            // Assert
            _auditLog.Verify(
                a => a.AddAsync(It.Is<AuditLog>(
                    log => log.Action == "Created" &&
                           log.EntityName == "Staff")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CommandWithSpecialisations_SetsSpecialisationsOnStaff()
        {
            // Arrange
            Staff? savedStaff = null;

            _staffRepo
                .Setup(r => r.AddAsync(It.IsAny<Staff>()))
                .Callback<Staff>(s => savedStaff = s)
                .Returns(Task.CompletedTask);

            var command = new CreateStaffCommand
            {
                FirstName = "Sipho",
                LastName = "Mahlangu",
                Phone = "0712345604",
                Role = "Colourist",
                Specialisations = new List<int> { 2, 4, 5 }   // service IDs
            };

            // Act
            await _handler.Handle(command);

            // Assert — specialisations must be set on the staff entity
            savedStaff.Should().NotBeNull();
            savedStaff!.GetSpecialisationIds().Should().BeEquivalentTo(new[] { 2, 4, 5 });
        }

        [Fact]
        public async Task Handle_CommandWithNoSpecialisations_DoesNotCallSetSpecialisations()
        {
            // Arrange — empty list — SetSpecialisations should not be called
            Staff? savedStaff = null;

            _staffRepo
                .Setup(r => r.AddAsync(It.IsAny<Staff>()))
                .Callback<Staff>(s => savedStaff = s)
                .Returns(Task.CompletedTask);

            var command = new CreateStaffCommand
            {
                FirstName = "Lerato",
                LastName = "Dlamini",
                Phone = "0712345602",
                Role = "Receptionist",
                Specialisations = new List<int>()   // ← empty
            };

            // Act
            await _handler.Handle(command);

            // Assert — no specialisations set so list should be empty
            savedStaff.Should().NotBeNull();
            savedStaff!.GetSpecialisationIds().Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CommandWithoutEmail_CreateStaffWithNullEmail()
        {
            // Arrange — email is optional for staff
            Staff? savedStaff = null;

            _staffRepo
                .Setup(r => r.AddAsync(It.IsAny<Staff>()))
                .Callback<Staff>(s => savedStaff = s)
                .Returns(Task.CompletedTask);

            var command = new CreateStaffCommand
            {
                FirstName = "Ayanda",
                LastName = "Mthembu",
                Phone = "0712345605",
                Role = "Therapist",
                Email = null,
                Specialisations = new List<int>()
            };

            // Act
            await _handler.Handle(command);

            // Assert — no exception, email stored as null
            savedStaff.Should().NotBeNull();
            savedStaff!.Email.Should().BeNull();
        }
    }
}
