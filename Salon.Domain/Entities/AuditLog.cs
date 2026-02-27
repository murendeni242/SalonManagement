namespace Salon.Domain.Entities;

/// <summary>
/// A single immutable record of any change made to any entity in the system.
///
/// ONE table for the whole system — Bookings, Services, Staff, Sales etc.
/// When you add auditing to other entities later, just inject IAuditLogRepository
/// into those handlers and use entityName = "Service" / "Staff" etc.
/// No new tables, no new classes.
///
/// EntityName + EntityId identify exactly what changed.
/// OldValues + NewValues are JSON snapshots of before and after.
/// Rows are never updated or deleted — only inserted.
/// </summary>
public class AuditLog
{
    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    /// <summary>
    /// Type of entity that changed, e.g. "Booking", "Service", "Staff".
    /// Used to filter history by entity type.
    /// </summary>
    public string EntityName { get; private set; } = default!;

    /// <summary>Primary key of the entity that changed.</summary>
    public int EntityId { get; private set; }

    /// <summary>
    /// Short label for what happened, e.g.
    /// "Created" | "Updated" | "Confirmed" | "Cancelled" | "Completed" | "SoftDeleted".
    /// </summary>
    public string Action { get; private set; } = default!;

    /// <summary>
    /// Human-readable sentence, e.g. "Booking #5 confirmed by owner@salon.com".
    /// </summary>
    public string Description { get; private set; } = default!;

    /// <summary>JSON snapshot of fields before the change. Null for Created entries.</summary>
    public string? OldValues { get; private set; }

    /// <summary>JSON snapshot of fields after the change. Null for SoftDeleted entries.</summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// Email of the user who triggered the change, read from the JWT claim.
    /// "System" when triggered outside an authenticated request.
    /// </summary>
    public string ChangedBy { get; private set; } = default!;

    /// <summary>UTC timestamp when this entry was written. Never changes.</summary>
    public DateTime ChangedAt { get; private set; }

    /// <summary>Required by EF Core. Do not call directly.</summary>
    protected AuditLog() { }

    /// <summary>
    /// Creates a new audit log entry. All fields are fixed at construction — nothing can change afterwards.
    /// </summary>
    /// <param name="entityName">Entity type that changed, e.g. "Booking".</param>
    /// <param name="entityId">Primary key of the entity that changed.</param>
    /// <param name="action">Short change label, e.g. "Confirmed".</param>
    /// <param name="description">Human-readable sentence of what happened.</param>
    /// <param name="changedBy">Email from the JWT of the acting user.</param>
    /// <param name="oldValues">JSON before the change, or null for Created entries.</param>
    /// <param name="newValues">JSON after the change, or null for SoftDeleted entries.</param>
    public AuditLog(string entityName, int entityId, string action,
        string description, string changedBy,
        string? oldValues = null, string? newValues = null)
    {
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        Description = description;
        ChangedBy = changedBy;
        OldValues = oldValues;
        NewValues = newValues;
        ChangedAt = DateTime.UtcNow;
    }
}