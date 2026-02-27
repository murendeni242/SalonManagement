using Salon.Application.DTOs;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Users;

/// <summary>
/// Returns all user accounts ordered by role then email.
/// Owner only — enforced at the controller level.
/// </summary>
public class GetUsersHandler
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> Handle()
    {
        var users = await _userRepository.GetAllAsync();

        return users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Email)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                Status = u.Status,
                MustChangePassword = u.MustChangePassword,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
            });
    }
}