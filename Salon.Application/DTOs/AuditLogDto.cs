namespace Salon.Application.DTOs;

/// <summary>
/// Read model returned by any GET .../audit endpoint in the system.
/// The same shape is used whether the history is for a Booking, Service, Staff, etc.
/// </summary>
public class AuditLogDto
{
    /// <summary>Primary key of this audit entry.</summary>
    public int Id { get; set; }

    /// <summary>Entity type that changed, e.g. "Booking", "Service".</summary>
    public string EntityName { get; set; } = default!;

    /// <summary>Primary key of the entity that changed.</summary>
    public int EntityId { get; set; }

    /// <summary>Type of change: Created | Updated | Confirmed | Cancelled | Completed | SoftDeleted.</summary>
    public string Action { get; set; } = default!;

    /// <summary>Human-readable sentence describing the change.</summary>
    public string Description { get; set; } = default!;

    /// <summary>JSON of fields before the change. Null for Created entries.</summary>
    public string? OldValues { get; set; }

    /// <summary>JSON of fields after the change. Null for SoftDeleted entries.</summary>
    public string? NewValues { get; set; }

    /// <summary>Email of the user who triggered the change.</summary>
    public string ChangedBy { get; set; } = default!;

    /// <summary>UTC timestamp when this entry was recorded.</summary>
    public DateTime ChangedAt { get; set; }
}