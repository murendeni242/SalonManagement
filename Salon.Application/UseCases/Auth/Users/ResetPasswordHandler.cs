using Salon.Application.Security;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Users;

/// <summary>
/// Generates a new temporary password for any user account.
/// The new password is returned once so the Owner can share it.
/// MustChangePassword is reset to true so the user must set a new password on next login.
/// Owner only — enforced at the controller level.
/// </summary>
public class ResetPasswordHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<string> Handle(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new ApplicationException("User not found.");

        var generatedPassword = PasswordGenerator.Generate();

        // Domain method — sets new hash AND sets MustChangePassword = true
        user.ResetPassword(_passwordHasher.Hash(generatedPassword));

        await _userRepository.SaveChangesAsync();

        return generatedPassword;
    }
}