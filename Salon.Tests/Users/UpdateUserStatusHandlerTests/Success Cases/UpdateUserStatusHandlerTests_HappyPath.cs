using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Auth.Users;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Users.UpdateUserStatusHandlerTests.Success_Cases
{
    public class UpdateUserStatusHandlerTests_HappyPath
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly UpdateUserStatusHandler _handler;

        public UpdateUserStatusHandlerTests_HappyPath()
        {
            _handler = new UpdateUserStatusHandler(_userRepo.Object);
        }

        [Fact]
        public async Task Handle_ActiveUser_DeactivateSetsStatusToInactive()
        {
            // Arrange
            var user = new UserBuilder()
                .WithId(2)
                .WithStatus("Active")
                .Build();

            _userRepo
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(user);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(2, "Inactive");

            // Assert — domain method Deactivate() sets Status = "Inactive"
            user.Status.Should().Be("Inactive");
        }

        [Fact]
        public async Task Handle_InactiveUser_ReactivateSetsStatusToActive()
        {
            // Arrange
            var user = new UserBuilder()
                .WithId(2)
                .WithStatus("Inactive")
                .Build();

            _userRepo
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(user);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(2, "Active");

            // Assert — domain method Reactivate() sets Status = "Active"
            user.Status.Should().Be("Active");
        }

        [Fact]
        public async Task Handle_ValidStatusChange_CallsSaveChangesAsync()
        {
            // Arrange
            var user = new UserBuilder().WithId(2).Build();

            _userRepo
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(user);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(2, "Inactive");

            // Assert
            _userRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
