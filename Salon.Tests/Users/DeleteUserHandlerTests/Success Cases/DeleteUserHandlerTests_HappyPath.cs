using Moq;
using Salon.Application.UseCases.Auth.Users;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Users.DeleteUserHandlerTests.Success_Cases
{
    public class DeleteUserHandlerTests_HappyPath
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteUserHandler _handler;

        public DeleteUserHandlerTests_HappyPath()
        {
            // Current user is Owner with Id=1 — deleting OTHER users (Id != 1)
            _currentUser
                .Setup(x => x.UserId)
                .Returns(1);

            _handler = new DeleteUserHandler(
                _userRepo.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_DifferentUser_CallsRemoveAndSaveChanges()
        {
            // Arrange — deleting user Id=5, current user is Id=1
            var user = new UserBuilder()
                .WithId(5)
                .Build();

            _userRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(user);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(5);

            // Assert — Remove() then SaveChangesAsync()
            _userRepo.Verify(r => r.Remove(user), Times.Once);
            _userRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
