namespace Salon.Application.UseCases.Auth.Users
{
    /// <summary>Input for activating or deactivating a user account.</summary>
    public class UpdateUserStatusCommand
    {
        /// <summary>New status: Active | Inactive.</summary>
        public string Status { get; set; } = default!;
    }
}
