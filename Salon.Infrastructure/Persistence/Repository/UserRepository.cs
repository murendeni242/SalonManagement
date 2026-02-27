using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository.
/// Handles persistence operations for User entities.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly SalonDbContext _context;

    public UserRepository(SalonDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking() // optimization for read-only queries
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
