namespace Salon.Application.UseCases.Auth.Users
{
    /// <summary>Input for the logged-in user changing their own password.</summary>
    public class ChangePasswordCommand
    {
        /// <summary>The user's current password (plain text for verification).</summary>
        public string CurrentPassword { get; set; } = default!;

        /// <summary>The new password the user wants to set (plain text, min 8 chars).</summary>
        public string NewPassword { get; set; } = default!;
    }

}
