namespace Salon.Domain.Interfaces;

/// <summary>
/// Provides the identity of the currently authenticated user.
/// Handlers use this to stamp the ChangedBy field on audit entries
/// without taking a direct dependency on HttpContext.
/// Implemented in Infrastructure, registered as Scoped in DI.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Email of the authenticated user read from the JWT Email claim.
    /// Returns "System" when called outside an authenticated HTTP request.
    /// </summary>
    string UserEmail { get; }

    /// <summary>
    /// Id of the authenticated user from the JWT Sub claim.
    /// Returns null or 0 when called outside an authenticated HTTP request.
    /// </summary>
    int UserId { get; }
}
