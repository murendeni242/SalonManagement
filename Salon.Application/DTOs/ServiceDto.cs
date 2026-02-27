namespace Salon.Application.DTOs.Services;

/// <summary>
/// Read model returned to API consumers for all service queries.
/// All write operations use separate Command objects.
/// </summary>
public class ServiceDto
{
    /// <summary>Database primary key.</summary>
    public int Id { get; set; }

    /// <summary>Name of the service.</summary>
    public string Name { get; set; } = default!;

    /// <summary>Optional description.</summary>
    public string Description { get; set; } = "";

    /// <summary>How long the service takes in minutes.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Selling price.</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Active or Inactive.</summary>
    public string Status { get; set; } = "Active";

    /// <summary>True when this service has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>UTC timestamp of the soft-delete, or null if not deleted.</summary>
    public DateTime? DeletedAt { get; set; }
}