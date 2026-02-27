namespace Salon.Application.Auth;

/// <summary>
/// Central place for all role name constants.
/// Use these on [Authorize] attributes instead of magic strings.
/// These must match exactly what gets written into the JWT Role claim at login.
/// </summary>
public static class Roles
{
    /// <summary>Full access — the salon owner.</summary>
    public const string Owner = "Owner";

    /// <summary>Can manage bookings. Cannot manage staff, services, or users.</summary>
    public const string Reception = "Reception";

    /// <summary>Read-only access to their own schedule.</summary>
    public const string Staff = "Staff";
}