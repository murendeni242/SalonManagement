using FluentAssertions;
using Moq;
using Salon.Application.Security;
using Salon.Application.UseCases.Users;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Users.ResetPasswordHandlerTests.Failure_Cases
{
    public class ResetPasswordHandlerTests_FailureCases
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly ResetPasswordHandler _handler;

        public ResetPasswordHandlerTests_FailureCases()
        {
            _hasher
                .Setup(h => h.Hash(It.IsAny<string>()))
                .Returns("new_hashed_pw");

            _handler = new ResetPasswordHandler(
                _userRepo.Object,
                _hasher.Object);
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

            // Assert — ApplicationException (not NotFoundException) matches real handler
            await act.Should()
                .ThrowAsync<ApplicationException>()
                .WithMessage("*not found*");
        }
    }
}
