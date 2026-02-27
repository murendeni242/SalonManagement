using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Salon.Domain.Interfaces;

namespace Salon.Infrastructure.Services;

/// <summary>
/// Reads the authenticated user's email from the current HTTP request JWT claims.
/// Registered as Scoped in DI — one instance per HTTP request.
///
/// Handlers use this to stamp the ChangedBy field on audit entries
/// without taking a direct dependency on HttpContext.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    /// <remarks>
    /// Reads the Email claim your LoginHandler writes into the JWT.
    /// Falls back to NameIdentifier, then "System" if called outside a request.
    /// </remarks>
    public string UserEmail =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "System";

    /// <inheritdoc />
    public int UserId
    {
        get
        {
            var idValue = _httpContextAccessor
                .HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(idValue, out var id) ? id : 0;
        }
    }

}