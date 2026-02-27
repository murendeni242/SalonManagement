using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for User accounts.
/// </summary>
public interface IUserRepository
{
    /// <summary>Returns a user by email, or null if not found.</summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Saves a new user to the database.</summary>
    Task AddAsync(User user);

    // Add these to your existing IUserRepository:
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    void Remove(User user);
    Task SaveChangesAsync();
}