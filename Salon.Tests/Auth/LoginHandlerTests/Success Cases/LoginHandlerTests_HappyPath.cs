using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Salon.Application.Security;
using Salon.Application.UseCases.Auth;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Auth.LoginHandlerTests.Success_Cases
{
    public class LoginHandlerTests_HappyPath
    {
        private const string ValidPassword = "Password1!";

        // ── Shared mocks ───────────────────────────────────────────────
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IPasswordHasher> _hasher = new();  // Salon.Application.Security — NOT Microsoft.AspNetCore.Identity
        private readonly IConfiguration _config;
        private readonly LoginHandler _handler;

        public LoginHandlerTests_HappyPath()
        {
            // Build a fake IConfiguration with the JWT values LoginHandler needs.
            // LoginHandler reads: Jwt:Key, Jwt:ExpiryMinutes, Jwt:Issuer, Jwt:Audience
            var configValues = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Jwt:Issuer"] = "SalonTest",
                ["Jwt:Audience"] = "SalonTestClient",
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            _handler = new LoginHandler(
                _userRepo.Object,
                _hasher.Object,
                _config);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsCorrectRoleAndEmail()
        {
            // Arrange
            var user = new UserBuilder()
                .WithEmail("owner@salon.co.za")
                .WithRole("Owner")
                .WithHashedPassword("hashed_pw")
                .Build();

            _userRepo
                .Setup(r => r.GetByEmailAsync("owner@salon.co.za"))
                .ReturnsAsync(user);

            _hasher
                .Setup(h => h.Verify(ValidPassword, "hashed_pw"))
                .Returns(true);

            var command = new LoginCommand
            {
                Email = "owner@salon.co.za",
                Password = ValidPassword
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert
            result.Role.Should().Be("Owner");
            result.Email.Should().Be("owner@salon.co.za");
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsNonEmptyJwtToken()
        {
            // Arrange
            var user = new UserBuilder()
                .WithEmail("owner@salon.co.za")
                .WithHashedPassword("hashed_pw")
                .Build();

            _userRepo
                .Setup(r => r.GetByEmailAsync("owner@salon.co.za"))
                .ReturnsAsync(user);

            _hasher
                .Setup(h => h.Verify(ValidPassword, "hashed_pw"))
                .Returns(true);

            var command = new LoginCommand
            {
                Email = "owner@salon.co.za",
                Password = ValidPassword
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert — handler generates the JWT internally from IConfiguration
            //          we verify it is non-empty and expiry is in the future
            result.Token.Should().NotBeNullOrWhiteSpace();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task Handle_ValidLogin_SetsLastLoginAt()
        {
            // Arrange
            var user = new UserBuilder()
                .WithEmail("owner@salon.co.za")
                .WithHashedPassword("hashed_pw")
                .Build();

            _userRepo
                .Setup(r => r.GetByEmailAsync("owner@salon.co.za"))
                .ReturnsAsync(user);

            _hasher
                .Setup(h => h.Verify(ValidPassword, "hashed_pw"))
                .Returns(true);

            var command = new LoginCommand
            {
                Email = "owner@salon.co.za",
                Password = ValidPassword
            };

            // Act
            await _handler.Handle(command);

            // Assert — LastLoginAt must be stamped on successful login
            user.LastLoginAt.Should().NotBeNull();
            user.LastLoginAt.Should().BeCloseTo(
                DateTime.UtcNow,
                precision: TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Handle_ValidLogin_CallsSaveChangesAsync()
        {
            // Arrange — LastLoginAt must be persisted to the database, not just set in memory
            var user = new UserBuilder()
                .WithEmail("owner@salon.co.za")
                .WithHashedPassword("hashed_pw")
                .Build();

            _userRepo
                .Setup(r => r.GetByEmailAsync("owner@salon.co.za"))
                .ReturnsAsync(user);

            _hasher
                .Setup(h => h.Verify(ValidPassword, "hashed_pw"))
                .Returns(true);

            var command = new LoginCommand
            {
                Email = "owner@salon.co.za",
                Password = ValidPassword
            };

            // Act
            await _handler.Handle(command);

            // Assert — SaveChangesAsync must be called to persist LastLoginAt
            _userRepo.Verify(
                r => r.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AccountWithMustChangePassword_ReturnsFlagAsTrue()
        {
            // Arrange — owner-created account, user has not yet set their own password
            var user = new UserBuilder()
                .WithEmail("newstaff@salon.co.za")
                .WithHashedPassword("temp_hash")
                .WithMustChangePassword(true)
                .Build();

            _userRepo
                .Setup(r => r.GetByEmailAsync("newstaff@salon.co.za"))
                .ReturnsAsync(user);

            _hasher
                .Setup(h => h.Verify(ValidPassword, "temp_hash"))
                .Returns(true);

            var command = new LoginCommand
            {
                Email = "newstaff@salon.co.za",
                Password = ValidPassword
            };

            // Act
            var result = await _handler.Handle(command);

            // Assert — frontend uses this flag to redirect to /change-password
            result.MustChangePassword.Should().BeTrue();
        }
    }

}

