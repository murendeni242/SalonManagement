using Salon.Domain.Common;

namespace Salon.Domain.Entities;

/// <summary>
/// Represents a salon service (e.g. Haircut, Colour, Treatment).
/// All business rules live here — no outside layer can put this into a bad state.
/// </summary>
public class Service
{
    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    /// <summary>Name of the service, e.g. "Haircut". Required, max 200 characters.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional description shown to customers.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>How long the service takes in minutes. Must be greater than 0.</summary>
    public int DurationMinutes { get; private set; }

    /// <summary>Selling price. Cannot be negative.</summary>
    public decimal BasePrice { get; private set; }

    /// <summary>Active or Inactive. Inactive services cannot be booked.</summary>
    public string Status { get; private set; } = "Active";

    /// <summary>
    /// True when this service has been soft-deleted.
    /// Soft-deleted services are hidden from normal queries but kept in the
    /// database so the audit trail and historical bookings are never broken.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>UTC timestamp of the soft-delete, or null if not deleted.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Required by EF Core. Do not call directly.</summary>
    protected Service() { }

    /// <summary>
    /// Creates a new active service.
    /// </summary>
    /// <param name="name">Service name. Required.</param>
    /// <param name="durationMinutes">Duration in minutes. Must be greater than 0.</param>
    /// <param name="basePrice">Selling price. Cannot be negative.</param>
    /// <param name="description">Optional description.</param>
    /// <exception cref="DomainException">Thrown when any business rule is violated.</exception>
    public Service(string name, int durationMinutes, decimal basePrice, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Service name is required.");
        if (durationMinutes <= 0)
            throw new DomainException("Duration must be greater than 0.");
        if (basePrice < 0)
            throw new DomainException("Base price cannot be negative.");

        Name = name;
        DurationMinutes = durationMinutes;
        BasePrice = basePrice;
        Description = description;
    }

    /// <summary>
    /// Updates the editable fields of this service.
    /// Can be called regardless of Status — even Inactive services can be corrected.
    /// Cannot be called on a soft-deleted service.
    /// </summary>
    /// <param name="name">New name. Required.</param>
    /// <param name="durationMinutes">New duration. Must be greater than 0.</param>
    /// <param name="basePrice">New price. Cannot be negative.</param>
    /// <param name="description">New description, or empty string to clear.</param>
    /// <exception cref="DomainException">Thrown when the service is deleted or a rule is violated.</exception>
    public void Update(string name, int durationMinutes, decimal basePrice, string description = "")
    {
        if (IsDeleted)
            throw new DomainException("Cannot update a deleted service.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Service name is required.");
        if (durationMinutes <= 0)
            throw new DomainException("Duration must be greater than 0.");
        if (basePrice < 0)
            throw new DomainException("Base price cannot be negative.");

        Name = name;
        DurationMinutes = durationMinutes;
        BasePrice = basePrice;
        Description = description;
    }

    /// <summary>
    /// Sets the service status to Active or Inactive.
    /// Inactive services should not be offered for new bookings.
    /// </summary>
    /// <param name="status">Must be "Active" or "Inactive".</param>
    /// <exception cref="DomainException">Thrown when the status value is not recognised.</exception>
    public void SetStatus(string status)
    {
        if (status != "Active" && status != "Inactive")
            throw new DomainException("Status must be Active or Inactive.");
        Status = status;
    }

    /// <summary>
    /// Soft-deletes the service. The row stays in the database so historical
    /// bookings and the audit trail are never broken.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the service is already deleted.</exception>
    public void SoftDelete()
    {
        if (IsDeleted)
            throw new DomainException("This service has already been deleted.");
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}