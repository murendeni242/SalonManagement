using Salon.Domain.Common;

namespace Salon.Domain.Entities;

/// <summary>
/// Represents a salon employee who performs services and is assigned to bookings.
///
/// Key design decisions for this salon system:
///
/// 1. SALON ROLE vs SYSTEM ROLE — these are two different things.
///    Staff.Role = the employee's JOB TITLE: Stylist | Colourist | Therapist | Manager | Receptionist.
///    User.Role  = their SYSTEM ACCESS LEVEL: Owner | Reception | Staff.
///    One person can be a "Stylist" (Staff.Role) with "Staff" system access (User.Role).
///    They are linked by matching email address.
///
/// 2. SPECIALISATIONS — a staff member has a set of service IDs they are qualified to perform.
///    A Colourist shouldn't be assigned to a massage booking.
///    Stored as a comma-separated string of service IDs (e.g. "1,3,7") — simple and queryable.
///    An empty Specialisations field means they can perform ALL services.
///
/// 3. SOFT DELETE — same pattern as Service. Historical bookings must never lose their
///    staff reference. IsDeleted excluded via EF Core global query filter.
///
/// 4. EMAIL is optional — not all staff need system login access — but must be
///    unique when provided, so it can safely link to a User account.
/// </summary>
public class Staff
{
    // ── Identity ──────────────────────────────────────────────────────

    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    // ── Personal details ──────────────────────────────────────────────

    /// <summary>First name. Required.</summary>
    public string FirstName { get; private set; } = default!;

    /// <summary>Last name. Required.</summary>
    public string LastName { get; private set; } = default!;

    /// <summary>Contact phone number.</summary>
    public string Phone { get; private set; } = default!;

    /// <summary>
    /// Optional work email. Used to link this Staff profile to a User login account.
    /// When a staff member logs in, their schedule is filtered to their StaffId.
    /// Must be unique when provided.
    /// </summary>
    public string? Email { get; private set; }

    // ── Salon role ────────────────────────────────────────────────────

    /// <summary>
    /// The employee's salon job title.
    /// Valid values: Stylist | Colourist | Therapist | Manager | Receptionist.
    /// This is NOT the system login role — see User.Role for that.
    /// </summary>
    public string Role { get; private set; } = default!;

    // ── Specialisations ───────────────────────────────────────────────

    /// <summary>
    /// Comma-separated Service IDs this staff member is qualified to perform.
    /// Empty string means they can perform ALL services — no restrictions.
    /// Example: "1,3,7" means they can only be assigned to services 1, 3 and 7.
    /// Managed via SetSpecialisations() — never set directly.
    /// </summary>
    public string Specialisations { get; private set; } = string.Empty;

    // ── Status + soft delete ──────────────────────────────────────────

    /// <summary>
    /// Active or Inactive.
    /// Inactive staff are not shown in booking forms and cannot be assigned to new appointments.
    /// </summary>
    public string Status { get; private set; } = "Active";

    /// <summary>
    /// True when this record has been soft-deleted by the Owner.
    /// Soft-deleted staff are hidden from all normal queries via EF Core global filter.
    /// The row is kept so historical bookings and the audit trail are never broken.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>UTC timestamp of the soft-delete, or null if still active.</summary>
    public DateTime? DeletedAt { get; private set; }

    // ── EF Core ───────────────────────────────────────────────────────

    /// <summary>Required by EF Core. Do not call directly.</summary>
    protected Staff() { }

    // ── Constructor ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a new active staff profile.
    /// </summary>
    /// <param name="firstName">First name. Required.</param>
    /// <param name="lastName">Last name. Required.</param>
    /// <param name="phone">Contact phone number.</param>
    /// <param name="role">Salon job title. Required. Stylist | Colourist | Therapist | Manager | Receptionist.</param>
    /// <param name="email">Optional work email. Must be unique in the system when provided.</param>
    /// <exception cref="DomainException">Thrown when any required field is blank.</exception>
    public Staff(string firstName, string lastName, string phone, string role, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(role))
            throw new DomainException("Role is required.");

        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Role = role;
        Email = NormaliseEmail(email);
    }

    // ── State-change methods ──────────────────────────────────────────

    /// <summary>
    /// Updates the staff member's personal and role details.
    /// Cannot be called on a soft-deleted record.
    /// </summary>
    /// <param name="firstName">New first name. Required.</param>
    /// <param name="lastName">New last name. Required.</param>
    /// <param name="phone">New contact number.</param>
    /// <param name="role">New salon role. Required.</param>
    /// <param name="email">New email, or null to clear.</param>
    /// <exception cref="DomainException">Thrown when the record is deleted or a required field is blank.</exception>
    public void Update(string firstName, string lastName, string phone, string role, string? email = null)
    {
        if (IsDeleted)
            throw new DomainException("Cannot update a deleted staff record.");
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(role))
            throw new DomainException("Role is required.");

        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Role = role;
        Email = NormaliseEmail(email);
    }

    /// <summary>
    /// Replaces the full set of service specialisations.
    /// Pass an empty list to indicate this staff member can perform all services.
    /// </summary>
    /// <param name="serviceIds">Collection of Service primary keys this person is qualified for.</param>
    /// <exception cref="DomainException">Thrown when the record is deleted.</exception>
    public void SetSpecialisations(IEnumerable<int> serviceIds)
    {
        if (IsDeleted)
            throw new DomainException("Cannot update a deleted staff record.");

        Specialisations = string.Join(",", serviceIds.Distinct().OrderBy(x => x));
    }

    /// <summary>
    /// Returns specialisation service IDs as a typed list.
    /// Returns an empty list when the staff member has no restrictions (all services allowed).
    /// </summary>
    public List<int> GetSpecialisationIds()
    {
        if (string.IsNullOrWhiteSpace(Specialisations))
            return new List<int>();

        return Specialisations
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();
    }

    /// <summary>
    /// Returns true when this staff member can perform the given service.
    /// A staff member with an empty specialisations list can perform ALL services.
    /// </summary>
    /// <param name="serviceId">Service ID to check.</param>
    public bool CanPerform(int serviceId)
    {
        var ids = GetSpecialisationIds();
        return ids.Count == 0 || ids.Contains(serviceId);
    }

    /// <summary>
    /// Sets the staff member's status to Active or Inactive.
    /// Inactive staff will not appear in the booking form staff picker.
    /// </summary>
    /// <param name="status">Must be "Active" or "Inactive".</param>
    /// <exception cref="DomainException">Thrown when the value is not recognised.</exception>
    public void SetStatus(string status)
    {
        if (status != "Active" && status != "Inactive")
            throw new DomainException("Status must be Active or Inactive.");
        Status = status;
    }

    /// <summary>
    /// Soft-deletes this staff record. The row stays in the database so historical
    /// bookings and the audit trail are never lost.
    /// </summary>
    /// <exception cref="DomainException">Thrown when already deleted.</exception>
    public void SoftDelete()
    {
        if (IsDeleted)
            throw new DomainException("This staff record has already been deleted.");
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    // ── Computed properties ───────────────────────────────────────────

    /// <summary>Full display name, e.g. "Murendeni Mulaudzi".</summary>
    public string FullName => $"{FirstName} {LastName}";

    // ── Private helpers ───────────────────────────────────────────────

    private static string? NormaliseEmail(string? email)
        => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
}