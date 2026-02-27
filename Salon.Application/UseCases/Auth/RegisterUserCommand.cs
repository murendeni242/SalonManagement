namespace Salon.Application.UseCases.Auth;

/// <summary>
/// Input for creating a new system user account.
/// </summary>
public class RegisterUserCommand
{
    /// <summary>Login email address. Must be unique.</summary>
    public string Email { get; set; } = default!;

    /// <summary>Plain-text password. Will be hashed before storage.</summary>
    public string Password { get; set; } = default!;

    /// <summary>
    /// Role to assign. Must be one of: Owner | Reception | Staff.
    /// Defaults to Owner for the initial setup.
    /// </summary>
    public string Role { get; set; } = "Owner";
}