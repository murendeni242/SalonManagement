using Salon.Application.DTOs;

namespace Salon.Application.UseCases.Auth.Users
{
    /// <summary>Returned after a user is created — includes the generated password shown once to Owner.</summary>
    public class CreateUserResult
    {
        public UserDto User { get; set; } = default!;
        public string GeneratedPassword { get; set; } = default!;
    }
}
