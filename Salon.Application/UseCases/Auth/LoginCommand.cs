namespace Salon.Application.UseCases.Auth;

/// <summary>Input for the login endpoint.</summary>
public class LoginCommand
{
    /// <summary>Registered email address.</summary>
    public string Email { get; set; } = default!;

    /// <summary>Plain-text password to verify against the stored hash.</summary>
    public string Password { get; set; } = default!;
}