using Salon.Application.Security;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Auth;

/// <summary>
/// Creates a new user account with a hashed password.
/// Blocks duplicate email addresses.
/// </summary>
public class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    /// <param name="userRepository">User persistence contract.</param>
    /// <param name="passwordHasher">BCrypt hasher, injected from DI.</param>
    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Registers a new user. Throws if the email is already taken.
    /// </summary>
    /// <param name="command">Registration details from the API.</param>
    /// <returns>The new user's database ID.</returns>
    /// <exception cref="ApplicationException">Thrown when the email is already registered.</exception>
    public async Task<int> Handle(RegisterUserCommand command)
    {
        var existing = await _userRepository.GetByEmailAsync(command.Email);
        if (existing != null)
            throw new ApplicationException("Email already registered.");

        var hash = _passwordHasher.Hash(command.Password);
        var user = new User(command.Email, hash, command.Role);

        await _userRepository.AddAsync(user);
        return user.Id;
    }
}