using Microsoft.AspNetCore.Identity;
using Salon.Application.Security;
using Salon.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon.Application.UseCases.Auth.Users
{
    /// <summary>
    /// Allows a logged-in user to change their own password.
    /// Verifies the current password before allowing the change.
    /// Also clears MustChangePassword — used when a user sets their
    /// own password after receiving a temporary one from the Owner.
    /// </summary>
    public class ChangePasswordHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUser;   // same pattern as your other handlers

        public ChangePasswordHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _currentUser = currentUser;
        }

        public async Task Handle(ChangePasswordCommand command)
        {
            var user = await _userRepository.GetByIdAsync(_currentUser.UserId)
                ?? throw new ApplicationException("User not found.");

            // Verify using IPasswordHasher — same interface as login
            if (!_passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
                throw new ApplicationException("Current password is incorrect.");

            if (command.NewPassword.Length < 8)
                throw new ApplicationException("New password must be at least 8 characters.");

            if (command.NewPassword == command.CurrentPassword)
                throw new ApplicationException("New password must be different from the current password.");

            // Domain method — sets new hash AND clears MustChangePassword flag
            user.SetPassword(_passwordHasher.Hash(command.NewPassword));

            await _userRepository.SaveChangesAsync();
        }
    }
}
