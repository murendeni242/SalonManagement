using Microsoft.AspNetCore.Identity;
using Salon.Application.Auth;
using Salon.Application.DTOs;
using Salon.Application.Security;
using Salon.Application.UseCases.Users;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using System.Linq;

namespace Salon.Application.UseCases.Auth.Users
{
    /// <summary>
    /// Creates a new user account with a system-generated temporary password.
    /// The generated password is returned once so the Owner can share it.
    /// MustChangePassword is set to true so the user is forced to set their
    /// own password on first login.
    /// </summary>
    public class CreateUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;  // same interface your RegisterUserHandler uses

        public CreateUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<CreateUserResult> Handle(CreateUserCommand command)
        {
            // Validate role — must match Roles constants exactly
            var validRoles = new[] { Roles.Owner, Roles.Reception, Roles.Staff };
            if (!validRoles.Contains(command.Role))
                throw new ApplicationException($"Invalid role '{command.Role}'. Must be Owner, Reception, or Staff.");

            // Block duplicate emails
            var existing = await _userRepository.GetByEmailAsync(command.Email);
            if (existing != null)
                throw new ApplicationException("A user with this email already exists.");

            // Generate a secure temporary password
            var generatedPassword = PasswordGenerator.Generate();

            // Use the same IPasswordHasher your RegisterUserHandler uses
            var hash = _passwordHasher.Hash(generatedPassword);

            // mustChangePassword: true — Owner-created accounts must set their own password
            var user = new User(
                email: command.Email,
                passwordHash: hash,
                role: command.Role,
                mustChangePassword: true
            );

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new CreateUserResult
            {
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role,
                    Status = user.Status,
                    MustChangePassword = user.MustChangePassword,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                },
                GeneratedPassword = generatedPassword,
            };
        }
    }
}
