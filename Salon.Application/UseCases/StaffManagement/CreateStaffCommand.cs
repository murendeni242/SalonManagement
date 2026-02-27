namespace Salon.Application.UseCases.StaffManagement;

/// <summary>
/// Input for creating a new staff profile. Owner role only.
/// </summary>
public class CreateStaffCommand
{
    /// <summary>First name. Required.</summary>
    public string FirstName { get; set; } = default!;

    /// <summary>Last name. Required.</summary>
    public string LastName { get; set; } = default!;

    /// <summary>Contact phone number.</summary>
    public string Phone { get; set; } = default!;

    /// <summary>
    /// Salon job title. Required.
    /// Stylist | Colourist | Therapist | Manager | Receptionist.
    /// </summary>
    public string Role { get; set; } = default!;

    /// <summary>
    /// Optional work email. Links this Staff profile to a User login account.
    /// Must be unique in the system when provided.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Optional service IDs this staff member is qualified to perform.
    /// Leave empty to indicate they can perform all services.
    /// </summary>
    public List<int> Specialisations { get; set; } = new();
}