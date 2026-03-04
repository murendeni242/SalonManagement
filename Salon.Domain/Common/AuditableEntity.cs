namespace Salon.Domain.Common;

public abstract class AuditableEntity : ISoftDeletable
{
    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>UserId of the user who created the record.</summary>
    public int CreatedBy { get; protected set; }

    /// <summary>UTC timestamp of the last update, or null if never updated.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>UserId of the user who last updated the record.</summary>
    public int? UpdatedBy { get; protected set; }

    /// <summary>True when the record has been soft-deleted.</summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>UTC timestamp of the soft-delete.</summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>UserId of the user who deleted the record.</summary>
    public int? DeletedBy { get; protected set; }

    /// <summary>
    /// Soft-deletes the entity. Called automatically by SalonDbContext
    /// when EF intercepts a Delete operation — do not call manually.
    /// </summary>
    internal void SoftDelete(int userId)
    {
        if (IsDeleted)
            throw new DomainException("This record has already been deleted.");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
    }

    internal void SetCreated(int userId)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
    }

    internal void SetUpdated(int userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    void ISoftDeletable.SoftDelete(int userId)
    {
        SoftDelete(userId);
    }
}