namespace Salon.Application.UseCases.Auth.Users
{
    /// <summary>Input for creating a new system login account.</summary>
    public class CreateUserCommand
    {
        /// <summary>Login email. Must be unique.</summary>
        public string Email { get; set; } = default!;

        /// <summary>System role: Owner | Reception | Staff.</summary>
        public string Role { get; set; } = default!;
    }
}
