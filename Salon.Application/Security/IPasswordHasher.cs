namespace Salon.Application.Security;

/// <summary>
/// Password hashing contract. Implementation uses BCrypt.
/// Lives in Application so handlers can depend on it without touching infrastructure.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Returns a BCrypt hash of the given plain-text password.</summary>
    string Hash(string password);

    /// <summary>Returns true when the plain-text password matches the stored hash.</summary>
    bool Verify(string password, string hash);
}