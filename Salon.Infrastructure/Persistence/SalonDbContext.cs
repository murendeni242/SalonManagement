using Microsoft.EntityFrameworkCore;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Salon application.
/// Only change from your original: one new DbSet for the shared AuditLogs table.
/// ApplyConfigurationsFromAssembly automatically picks up all IEntityTypeConfiguration classes.
/// </summary>
public class SalonDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;

    public SalonDbContext(DbContextOptions<SalonDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Shared audit log table. One table for the whole system.
    /// Rows are append-only — never updated or deleted.
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreated(userId);
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdated(userId);
                    // Protect original creation fields from being overwritten
                    entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // Soft-delete for entities that support it
                    // IExcludeFromSoftDelete opts an entity out (e.g. Sale)
                    if (entry.Entity is ISoftDeletable softDeletable)
                    {
                        entry.State = EntityState.Modified;
                        softDeletable.SoftDelete(userId);
                    }
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalonDbContext).Assembly);

        modelBuilder.Entity<Booking> ().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Staff>   ().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Service> ().HasQueryFilter(e => !e.IsDeleted);
    }
}