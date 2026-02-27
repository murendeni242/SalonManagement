using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.Auth.Users;
using Salon.Application.UseCases.Users;

namespace Salon.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly GetUsersHandler _getUsers;
    private readonly CreateUserHandler _createUser;
    private readonly ResetPasswordHandler _resetPassword;
    private readonly ChangePasswordHandler _changePassword;
    private readonly UpdateUserStatusHandler _updateStatus;
    private readonly DeleteUserHandler _deleteUser;

    public UsersController(
        GetUsersHandler getUsers,
        CreateUserHandler createUser,
        ResetPasswordHandler resetPassword,
        ChangePasswordHandler changePassword,
        UpdateUserStatusHandler updateStatus,
        DeleteUserHandler deleteUser)
    {
        _getUsers = getUsers;
        _createUser = createUser;
        _resetPassword = resetPassword;
        _changePassword = changePassword;
        _updateStatus = updateStatus;
        _deleteUser = deleteUser;
    }

    // GET /api/users — Owner only
    [HttpGet]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _getUsers.Handle();
        return Ok(users);
    }

    // POST /api/users — Owner only
    // Returns the generated password once — frontend shows it to the Owner
    [HttpPost]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
    {
        var result = await _createUser.Handle(command);
        return Ok(new
        {
            user = result.User,
            generatedPassword = result.GeneratedPassword,
        });
    }

    // POST /api/users/{id}/reset-password — Owner only
    [HttpPost("{id:int}/reset-password")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var generatedPassword = await _resetPassword.Handle(id);
        return Ok(new { generatedPassword });
    }

    // POST /api/users/change-password — any logged-in user (their own password)
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        await _changePassword.Handle(command);
        return Ok(new { message = "Password changed successfully." });
    }

    // PATCH /api/users/{id}/status — Owner only
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusCommand command)
    {
        await _updateStatus.Handle(id, command.Status);
        return Ok(new { message = $"User {command.Status.ToLower()}d successfully." });
    }

    // DELETE /api/users/{id} — Owner only
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Delete(int id)
    {
        await _deleteUser.Handle(id);
        return Ok(new { message = "User account deleted." });
    }
}