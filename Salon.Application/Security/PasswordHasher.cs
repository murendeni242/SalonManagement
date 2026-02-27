namespace Salon.Application.Security;

/// <summary>
/// BCrypt implementation of IPasswordHasher.
/// Registered as Scoped in DI via Program.cs.
/// Requires: dotnet add package BCrypt.Net-Next (in Salon.Application)
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc />
    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    /// <inheritdoc />
    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}