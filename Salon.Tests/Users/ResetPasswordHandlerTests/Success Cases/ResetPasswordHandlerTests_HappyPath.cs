
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Salon.Application.Security;
using Salon.Application.UseCases.Users;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Users.ResetPasswordHandlerTests.Success_Cases
{
    public class ResetPasswordHandlerTests_HappyPath
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly ResetPasswordHandler _handler;

        public ResetPasswordHandlerTests_HappyPath()
        {
            _hasher
                .Setup(h => h.Hash(It.IsAny<string>()))
                .Returns("new_hashed_pw");

            _handler = new ResetPasswordHandler(
                _userRepo.Object,
                _hasher.Object);
        }

        [Fact]
        public async Task Handle_ExistingUser_ReturnsNewGeneratedPassword()
        {
            // Arrange
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
            var result = await _handler.Handle(5);

            // Assert — owner must receive the new temp password to give to the employee
            result.Should().NotBeNullOrWhiteSpace();
            result.Length.Should().BeGreaterThanOrEqualTo(8);
        }

        [Fact]
        public async Task Handle_ExistingUser_SetsMustChangePasswordBackToTrue()
        {
            // Arrange — user previously cleared their flag, now being reset by Owner
            var user = new UserBuilder()
                .WithId(5)
                .WithMustChangePassword(false)
                .Build();

            _userRepo
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(user);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(5);

            // Assert — user must be forced to change password on next login
            user.MustChangePassword.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingUser_CallsSaveChangesAsync()
        {
            // Arrange
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

            // Assert — new password hash must be persisted
            _userRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
