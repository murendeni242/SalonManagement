using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Salon.Application.Security;
using Salon.Application.UseCases.Auth.Users;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Users.CreateUserHandlerTests.Failure_Cases
{
    public class CreateUserHandlerTests_FailureCases
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();
        private readonly CreateUserHandler _handler;

        public CreateUserHandlerTests_FailureCases()
        {
            _hasher
                .Setup(h => h.Hash(It.IsAny<string>()))
                .Returns("hashed_generated_pw");

            _handler = new CreateUserHandler(
                _userRepo.Object,
                _hasher.Object);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ThrowsApplicationException()
        {
            // Arrange — email is already registered
            var existingUser = new UserBuilder()
                .WithEmail("taken@salon.co.za")
                .Build();

            _userRepo
                .Setup(r => r.GetByEmailAsync("taken@salon.co.za"))
                .ReturnsAsync(existingUser);

            var command = new CreateUserCommand
            {
                Email = "taken@salon.co.za",
                Role = "Reception"
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<ApplicationException>()
                .WithMessage("*already exists*");
        }

        [Fact]
        public async Task Handle_InvalidRole_ThrowsApplicationException()
        {
            // Arrange
            _userRepo
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var command = new CreateUserCommand
            {
                Email = "new@salon.co.za",
                Role = "Janitor"   // not a valid system role
            };

            // Act
            var act = () => _handler.Handle(command);

            // Assert
            await act.Should()
                .ThrowAsync<ApplicationException>()
                .WithMessage("*Invalid role*");
        }

        [Fact]
        public async Task Handle_DuplicateEmail_NeverCallsAddAsync()
        {
            // Arrange — if email is taken, no user should be created at all
            var existingUser = new UserBuilder()
                .WithEmail("taken@salon.co.za")
                .Build();

            _userRepo
                .Setup(r => r.GetByEmailAsync("taken@salon.co.za"))
                .ReturnsAsync(existingUser);

            var command = new CreateUserCommand
            {
                Email = "taken@salon.co.za",
                Role = "Staff"
            };

            // Act
            try { await _handler.Handle(command); } catch { /* expected */ }

            // Assert — AddAsync must never be called when email is duplicate
            _userRepo.Verify(
                r => r.AddAsync(It.IsAny<User>()),
                Times.Never);
        }
    }
}
