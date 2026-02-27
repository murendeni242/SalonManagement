using Salon.Domain.Interfaces;
namespace Salon.Application.UseCases.Auth.Users
{
    /// <summary>
    /// Activates or deactivates a user account.
    /// Deactivated users receive a clear error message when they try to log in.
    /// Owner only — enforced at the controller level.
    /// </summary>
    public class UpdateUserStatusHandler
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserStatusHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Handle(int userId, string status)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new ApplicationException("User not found.");

            // Use domain methods — keeps mutation logic inside the entity
            if (status == "Inactive")
                user.Deactivate();
            else if (status == "Active")
                user.Reactivate();
            else
                throw new ApplicationException("Status must be Active or Inactive.");

            await _userRepository.SaveChangesAsync();
        }
    }
}
