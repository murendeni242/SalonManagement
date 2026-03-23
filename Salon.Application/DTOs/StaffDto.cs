namespace Salon.Application.DTOs.Staff;

/// <summary>
/// Read model for a staff member returned to API consumers.
/// </summary>
public class StaffDto
{
    /// <summary>Database primary key.</summary>
    public int Id { get; set; }

    /// <summary>First name.</summary>
    public string FirstName { get; set; } = default!;

    /// <summary>Last name.</summary>
    public string LastName { get; set; } = default!;

    /// <summary>Full display name, e.g. "Jane Smith".</summary>
    public string FullName { get; set; } = default!;

    /// <summary>Contact phone number.</summary>
    public string Phone { get; set; } = default!;

    /// <summary>Work email. Null when not set.</summary>
    public string? Email { get; set; }

    /// <summary>
    /// Salon job title: Stylist | Colourist | Therapist | Manager | Receptionist.
    /// </summary>
    public string Role { get; set; } = default!;

    /// <summary>Active or Inactive.</summary>
    public string Status { get; set; } = default!;

    /// <summary>
    /// Service IDs this staff member is specialised in.
    /// Empty list means they can perform all services.
    /// </summary>
    public List<int> Specialisations { get; set; } = new();

    /// <summary>True when this record has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>UTC timestamp of the soft-delete, or null if still active.</summary>
    public DateTime? DeletedAt { get; set; }
}