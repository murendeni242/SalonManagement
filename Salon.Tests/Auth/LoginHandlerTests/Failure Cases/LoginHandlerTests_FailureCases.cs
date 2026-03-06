using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Salon.Application.Security;
using Salon.Application.UseCases.Auth;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Tests.Helpers;

namespace Salon.Tests.Auth.LoginHandlerTests.Failure_Cases;

public class LoginHandlerTests_FailureCases
{
    private const string ValidPassword = "Password1!";

    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly IConfiguration _config;
    private readonly LoginHandler _handler;

    public LoginHandlerTests_FailureCases()
    {
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
    public async Task Handle_EmailNotRegistered_ThrowsApplicationException()
    {
        // Arrange — email does not exist in the database
        _userRepo
            .Setup(r => r.GetByEmailAsync("ghost@salon.co.za"))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand
        {
            Email = "ghost@salon.co.za",
            Password = ValidPassword
        };

        // Act
        var act = () => _handler.Handle(command);

        // Assert — same generic message for wrong email AND wrong password
        //          callers cannot determine which one was incorrect
        await act.Should()
            .ThrowAsync<ApplicationException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsApplicationException()
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
            .Setup(h => h.Verify("WrongPass!", "hashed_pw"))
            .Returns(false);    // ← hash does not match

        var command = new LoginCommand
        {
            Email = "owner@salon.co.za",
            Password = "WrongPass!"
        };

        // Act
        var act = () => _handler.Handle(command);

        // Assert — same message as wrong email (no hints to attackers)
        await act.Should()
            .ThrowAsync<ApplicationException>()
            .WithMessage("*Invalid credentials*");
    }

    [Fact]
    public async Task Handle_DeactivatedAccount_ThrowsApplicationException()
    {
        // Arrange — account was deactivated by Owner
        var user = new UserBuilder()
            .WithEmail("fired@salon.co.za")
            .WithHashedPassword("hashed_pw")
            .WithStatus("Inactive")
            .Build();

        _userRepo
            .Setup(r => r.GetByEmailAsync("fired@salon.co.za"))
            .ReturnsAsync(user);

        _hasher
            .Setup(h => h.Verify(ValidPassword, "hashed_pw"))
            .Returns(true);     // correct password — but account is inactive

        var command = new LoginCommand
        {
            Email = "fired@salon.co.za",
            Password = ValidPassword
        };

        // Act
        var act = () => _handler.Handle(command);

        // Assert — inactive accounts get a specific message (different from "Invalid credentials")
        await act.Should()
            .ThrowAsync<ApplicationException>()
            .WithMessage("*deactivated*");
    }
}