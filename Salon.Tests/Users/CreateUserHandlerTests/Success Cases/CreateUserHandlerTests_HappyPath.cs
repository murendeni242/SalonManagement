
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Salon.Application.Security;
using Salon.Application.UseCases.Auth.Users;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Tests.Users.CreateUserHandlerTests.Success_Cases
{
    public class CreateUserHandlerTests_HappyPath
    {
        // ── Shared mocks ───────────────────────────────────────────────
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly CreateUserHandler _handler;

        public CreateUserHandlerTests_HappyPath()
        {
            _hasher
                .Setup(h => h.Hash(It.IsAny<string>()))
                .Returns("hashed_generated_pw");

            _handler = new CreateUserHandler(
                _userRepo.Object,
                _hasher.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsGeneratedPassword()
        {
            // Arrange
            _userRepo
                .Setup(r => r.GetByEmailAsync("new@salon.co.za"))
                .ReturnsAsync((User?)null);  // email not taken

            _userRepo
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var command = new CreateUserCommand
            {
                Email = "new@salon.co.za",
                Role = "Reception"
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert — temporary password returned so Owner can hand it to the employee
            result.GeneratedPassword.Should().NotBeNullOrWhiteSpace();
            result.GeneratedPassword.Length.Should().BeGreaterThanOrEqualTo(8);
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesAccountWithMustChangePasswordTrue()
        {
            // Arrange
            User? savedUser = null;

            _userRepo
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _userRepo
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Callback<User>(u => savedUser = u)  // capture the user that was saved
                .Returns(Task.CompletedTask);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var command = new CreateUserCommand
            {
                Email = "nomsa@salon.co.za",
                Role = "Staff"
            };

            // Act
            await _handler.Handle(command);

            // Assert — new accounts must force a password change on first login
            savedUser.Should().NotBeNull();
            savedUser!.MustChangePassword.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnedUserDtoMatchesCommand()
        {
            // Arrange
            _userRepo
                .Setup(r => r.GetByEmailAsync("nomsa@salon.co.za"))
                .ReturnsAsync((User?)null);

            _userRepo
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var command = new CreateUserCommand
            {
                Email = "nomsa@salon.co.za",
                Role = "Staff"
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert — UserDto in response must reflect what was created
            result.User.Email.Should().Be("nomsa@salon.co.za");
            result.User.Role.Should().Be("Staff");
        }

        [Theory]
        [InlineData("Owner")]
        [InlineData("Reception")]
        [InlineData("Staff")]
        public async Task Handle_ValidRole_CreatesUserSuccessfully(string role)
        {
            // Arrange
            _userRepo
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _userRepo
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var command = new CreateUserCommand
            {
                Email = "staff@salon.co.za",
                Role = role
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert — all three valid roles must succeed without exception
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_ValidRequest_CallsSaveChangesAsync()
        {
            // Arrange
            _userRepo
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            _userRepo
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepo
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var command = new CreateUserCommand
            {
                Email = "new@salon.co.za",
                Role = "Reception"
            };

            // Act
            await _handler.Handle(command);

            // Assert — user must be persisted
            _userRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
