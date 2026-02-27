namespace Salon.Application.UseCases.Services;

/// <summary>
/// Input for creating a new service. Owner role only.
/// </summary>
public class CreateServiceCommand
{
    /// <summary>Service name. Required.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Duration in minutes. Must be greater than 0.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>Selling price. Cannot be negative.</summary>
    public decimal BasePrice { get; set; }
}