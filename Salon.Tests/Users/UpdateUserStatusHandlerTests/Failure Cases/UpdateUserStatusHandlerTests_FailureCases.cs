using FluentAssertions;
using Moq;
using Salon.Application.UseCases.Auth.Users;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Users.UpdateUserStatusHandlerTests.Failure_Cases
{
    public class UpdateUserStatusHandlerTests_FailureCases
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly UpdateUserStatusHandler _handler;

        public UpdateUserStatusHandlerTests_FailureCases()
        {
            _handler = new UpdateUserStatusHandler(_userRepo.Object);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsApplicationException()
        {
            // Arrange
            _userRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((User?)null);

            // Act
            var act = () => _handler.Handle(99, "Inactive");

            // Assert
            await act.Should()
                .ThrowAsync<ApplicationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_InvalidStatus_ThrowsApplicationException()
        {
            // Arrange
            var user = new UserBuilder().WithId(2).Build();

            _userRepo
                .Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(user);

            // Act — "Suspended" is not a valid status
            var act = () => _handler.Handle(2, "Suspended");

            // Assert
            await act.Should()
                .ThrowAsync<ApplicationException>()
                .WithMessage("*Active or Inactive*");
        }
    }
}
