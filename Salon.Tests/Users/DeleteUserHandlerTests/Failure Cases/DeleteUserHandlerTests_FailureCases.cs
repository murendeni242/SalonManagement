using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Auth.Users;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Users.DeleteUserHandlerTests.Failure_Cases
{
    public class DeleteUserHandlerTests_FailureCases
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly DeleteUserHandler _handler;

        public DeleteUserHandlerTests_FailureCases()
        {
            _currentUser
                .Setup(x => x.UserId)
                .Returns(1);

            _handler = new DeleteUserHandler(
                _userRepo.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task Handle_OwnAccount_ThrowsApplicationException()
        {
            // Arrange — trying to delete self (userId == currentUser.UserId == 1)
            // Act
            var act = () => _handler.Handle(1);

            // Assert — Owner cannot delete their own account
            await act.Should()
                .ThrowAsync<ApplicationException>()
                .WithMessage("*own account*");
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsApplicationException()
        {
            // Arrange
            _userRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((User?)null);

            // Act
            var act = () => _handler.Handle(99);

            // Assert
            await act.Should()
                .ThrowAsync<ApplicationException>()
                .WithMessage("*not found*");
        }
    }
}
