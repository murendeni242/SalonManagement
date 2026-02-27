using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.Auth;

namespace Salon.API.Controllers;

/// <summary>
/// Handles user registration and login.
///
/// Changes from original:
/// - Register is now [Authorize(Roles = Owner)] — only the Owner can create new accounts.
///   This prevents anyone from creating an Owner account via the API.
/// - Login now returns LoginResponse (token + role + email + expiresAt) instead of
///   just the token string. Frontend can use role immediately without decoding the JWT.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerHandler;
    private readonly LoginHandler _loginHandler;

    public AuthController(RegisterUserHandler registerHandler, LoginHandler loginHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
    }

    // POST api/auth/register
    /// <summary>
    /// Creates a new system user account. Owner role only.
    /// The Owner uses this to create Reception and Staff login accounts.
    /// The initial Owner account is created via database seed — not this endpoint.
    /// </summary>
    /// <response code="201">User created. Returns the new user ID.</response>
    /// <response code="400">Email already registered.</response>
    /// <response code="401">No JWT provided.</response>
    /// <response code="403">Caller is not Owner.</response>
    [HttpPost("register")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var userId = await _registerHandler.Handle(command);
        return Created("", new { userId });
    }

    // POST api/auth/login
    /// <summary>
    /// Validates credentials and returns a signed JWT plus user context.
    /// The response includes role, email, and expiry — frontend does not need to decode the JWT.
    /// </summary>
    /// <response code="200">Login successful. Returns token, role, email, expiresAt.</response>
    /// <response code="400">Invalid email or password.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var response = await _loginHandler.Handle(command);
        return Ok(response);
    }
}