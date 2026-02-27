using Salon.Domain.Common;

namespace Salon.Domain.Entities;

/// <summary>
/// A system login account. This is NOT the same as Staff.
/// Staff is a salon employee. User is who can log into the system.
/// One person can have both a Staff record and a User account.
/// </summary>
public class User
{
    // ── Identity ──────────────────────────────────────────────────

    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    /// <summary>Login email address. Must be unique across all users.</summary>
    public string Email { get; private set; } = default!;

    /// <summary>BCrypt hashed password. Never store plain text.</summary>
    public string PasswordHash { get; private set; } = default!;

    /// <summary>System role: Owner | Reception | Staff.</summary>
    public string Role { get; private set; } = default!;

    // ── Account state ─────────────────────────────────────────────

    /// <summary>
    /// Account status: Active | Inactive.
    /// Kept as a string (not enum) so it survives DB migrations cleanly
    /// and matches the rest of the domain pattern in this codebase.
    /// </summary>
    public string Status { get; private set; } = "Active";

    /// <summary>
    /// True when the account was created by an Owner and the user
    /// has not yet chosen their own password. Forces redirect to
    /// /change-password on first login.
    /// </summary>
    public bool MustChangePassword { get; private set; } = false;

    /// <summary>UTC timestamp when this account was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>UTC timestamp of the most recent successful login. Null if never logged in.</summary>
    public DateTime? LastLoginAt { get; private set; }

    // ── EF Core ───────────────────────────────────────────────────

    /// <summary>Required by EF Core. Do not call directly.</summary>
    protected User() { }

    // ── Constructor ───────────────────────────────────────────────

    /// <summary>
    /// Creates a new user account with a generated (temporary) password.
    /// MustChangePassword defaults to true so the user is forced to
    /// set their own password on first login.
    /// </summary>
    /// <param name="email">Login email. Must not be blank.</param>
    /// <param name="passwordHash">BCrypt hash of the temporary password.</param>
    /// <param name="role">Role string from Salon.Application.Auth.Roles constants.</param>
    /// <param name="mustChangePassword">
    /// True when the Owner creates the account (user must set their own password).
    /// False when the user registers themselves.
    /// </param>
    public User(string email, string passwordHash, string role, bool mustChangePassword = false)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password is required.");
        if (string.IsNullOrWhiteSpace(role))
            throw new DomainException("Role is required.");

        Email = email.ToLowerInvariant().Trim();
        PasswordHash = passwordHash;
        Role = role;
        Status = "Active";
        MustChangePassword = mustChangePassword;
        CreatedAt = DateTime.UtcNow;
        LastLoginAt = null;
    }

    // ── Domain methods ────────────────────────────────────────────

    /// <summary>Updates the password hash and clears the forced-change flag.</summary>
    public void SetPassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
        MustChangePassword = false;
    }

    /// <summary>
    /// Resets the password to a new generated hash and forces the user
    /// to change it again on next login.
    /// </summary>
    public void ResetPassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
        MustChangePassword = true;
    }

    /// <summary>Records the time of a successful login.</summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>Deactivates the account. Deactivated users cannot log in.</summary>
    public void Deactivate()
    {
        Status = "Inactive";
    }

    /// <summary>Reactivates a previously deactivated account.</summary>
    public void Reactivate()
    {
        Status = "Active";
    }
}