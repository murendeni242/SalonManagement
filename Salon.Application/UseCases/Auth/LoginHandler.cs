using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Salon.Application.Security;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Auth;

/// <summary>
/// Validates login credentials and returns a signed JWT plus user context.
/// The response includes role, email, and expiry so the frontend
/// never needs to decode the JWT itself.
/// </summary>
public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _config;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IConfiguration config)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    /// <summary>
    /// Authenticates credentials and returns a <see cref="LoginResponse"/>.
    /// Returns the same "Invalid credentials" message for both wrong email
    /// and wrong password — callers cannot determine which was incorrect.
    /// </summary>
    /// <param name="command">Login credentials from the API.</param>
    /// <returns>JWT token plus user context needed immediately by the frontend.</returns>
    /// <exception cref="ApplicationException">Thrown when credentials are invalid.</exception>
    public async Task<LoginResponse> Handle(LoginCommand command)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email)
            ?? throw new ApplicationException("Invalid credentials.");

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new ApplicationException("Invalid credentials.");

        if (user.Status == "Inactive")
            throw new ApplicationException("Your account has been deactivated. Please contact the salon owner.");

        user.RecordLogin();
        await _userRepository.SaveChangesAsync();

        var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"]!);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var token = GenerateJwt(user, expiresAt);

        return new LoginResponse
        {
            Token = token,
            Role = user.Role,
            Email = user.Email,
            ExpiresAt = expiresAt,
            MustChangePassword = user.MustChangePassword,
        };
    }

    private string GenerateJwt(Domain.Entities.User user, DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,   user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}