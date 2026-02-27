namespace Salon.Application.UseCases.Services;

/// <summary>
/// Input for updating an existing service. Owner role only.
/// Soft-deleted services cannot be updated.
/// </summary>
public class UpdateServiceCommand
{
    /// <summary>Primary key of the service to update. Set from the URL route parameter.</summary>
    public int Id { get; set; }

    /// <summary>New name. Required.</summary>
    public string Name { get; set; } = null!;

    /// <summary>New description, or empty to clear.</summary>
    public string? Description { get; set; }

    /// <summary>New duration in minutes. Must be greater than 0.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>New price. Cannot be negative.</summary>
    public decimal BasePrice { get; set; }

    /// <summary>New status. Must be "Active" or "Inactive".</summary>
    public string Status { get; set; } = "Active";
}