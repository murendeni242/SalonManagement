using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Auth.Users
{
    /// <summary>
    /// Permanently deletes a user account.
    /// Prevents an Owner from deleting their own account.
    /// Owner only — enforced at the controller level.
    /// </summary>
    public class DeleteUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;

        public DeleteUserHandler(
            IUserRepository userRepository,
            ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
        }

        public async Task Handle(int userId)
        {
            if (userId == _currentUser.UserId)
                throw new ApplicationException("You cannot delete your own account.");

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new ApplicationException("User not found.");

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
        }
    }
}
