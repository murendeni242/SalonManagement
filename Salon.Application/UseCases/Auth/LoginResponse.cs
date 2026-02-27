using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Salon.Application.Security;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Auth;

/// <summary>
/// Login response returned to the client.
/// Contains everything the frontend needs immediately after login
/// so it does not have to decode the JWT manually.
/// </summary>
public class LoginResponse
{
    /// <summary>Signed JWT to send as Authorization: Bearer {token} on subsequent requests.</summary>
    public string Token { get; set; } = default!;

    /// <summary>
    /// The user's system role: Owner | Reception | Staff.
    /// Frontend uses this to show/hide nav items and protect routes.
    /// </summary>
    public string Role { get; set; } = default!;

    /// <summary>Login email — useful to display "Logged in as owner@salon.com" in the UI.</summary>
    public string Email { get; set; } = default!;

    /// <summary>UTC timestamp when this token expires. Frontend can use this to auto-logout.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// True when the account was created by an Owner and the user has not yet chosen their own password.
    /// </summary>
    public bool MustChangePassword { get; set; }
}