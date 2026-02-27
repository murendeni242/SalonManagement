using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;

namespace Salon.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Salon application.
/// Only change from your original: one new DbSet for the shared AuditLogs table.
/// ApplyConfigurationsFromAssembly automatically picks up all IEntityTypeConfiguration classes.
/// </summary>
public class SalonDbContext : DbContext
{
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

    public SalonDbContext(DbContextOptions<SalonDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalonDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}