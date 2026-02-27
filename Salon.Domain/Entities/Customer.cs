using Salon.Domain.Common;

namespace Salon.Domain.Entities;

/// <summary>
/// Represents a salon customer — the central entity of the whole system.
/// Every booking and every payment traces back to a customer record.
///
/// Key design decisions for this salon system:
///
/// 1. SOFT DELETE — same pattern as Staff and Service. Customer records must
///    never be hard-deleted because their bookings and payments exist in the database.
///    Deleting the customer would orphan those records.
///
/// 2. NOTES — salons keep important client-specific information:
///    allergies, colour formulas, product preferences, medical notes.
///    A plain text field is flexible enough and avoids over-engineering.
///
/// 3. EMAIL is optional — walk-in customers often don't provide one.
///    When provided it must be unique so it can be used as a lookup key.
///
/// 4. PHONE is the primary lookup field at reception — "what's your number?"
///    is how 90% of salon check-ins work.
///
/// 5. DATE OF BIRTH is optional — useful for birthday promotions and
///    verifying customer identity when they call.
/// </summary>
public class Customer
{
    // ── Identity ──────────────────────────────────────────────────────

    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    // ── Personal details ──────────────────────────────────────────────

    /// <summary>First name. Required.</summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>Last name. Required.</summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Primary contact phone number.
    /// This is the main lookup field used by reception when a customer arrives.
    /// </summary>
    public string Phone { get; private set; } = string.Empty;

    /// <summary>
    /// Optional email address. Must be unique when provided.
    /// Used for appointment reminders and marketing communications.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Optional date of birth. Used for birthday promotions and identity verification.
    /// Stored as date only — no time component needed.
    /// </summary>
    public DateTime? DateOfBirth { get; private set; }

    // ── Salon-specific ────────────────────────────────────────────────

    /// <summary>
    /// Freetext notes about this customer written by staff.
    /// Used for: allergies, colour formulas, product preferences, medical notes,
    /// e.g. "Allergic to PPD. Always use X colour brand. Prefers quiet stylist."
    /// Maximum 2000 characters.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// UTC timestamp of the customer's most recent completed appointment.
    /// Updated automatically whenever a booking for this customer is completed.
    /// Used for "lapsed customer" reports (not visited in 90+ days).
    /// </summary>
    public DateTime? LastVisitAt { get; private set; }

    // ── Status + soft delete ──────────────────────────────────────────

    /// <summary>
    /// True when this customer record has been soft-deleted.
    /// Hidden from normal queries but kept so historical bookings are never orphaned.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>UTC timestamp of the soft-delete, or null if still active.</summary>
    public DateTime? DeletedAt { get; private set; }

    // ── EF Core ───────────────────────────────────────────────────────

    /// <summary>Required by EF Core. Do not call directly.</summary>
    protected Customer() { }

    // ── Constructor ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a new customer record.
    /// </summary>
    /// <param name="firstName">First name. Required.</param>
    /// <param name="lastName">Last name. Required.</param>
    /// <param name="phone">Contact phone. Required — main lookup field at reception.</param>
    /// <param name="email">Optional email. Must be unique when provided.</param>
    /// <param name="dateOfBirth">Optional date of birth for birthday promotions.</param>
    /// <exception cref="DomainException">Thrown when any required field is blank.</exception>
    public Customer(string firstName, string lastName, string phone,
        string? email = null, DateTime? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Phone number is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone.Trim();
        Email = NormaliseEmail(email);
        DateOfBirth = dateOfBirth?.Date;
    }

    // ── State-change methods ──────────────────────────────────────────

    /// <summary>
    /// Updates the customer's personal details.
    /// Cannot be called on a soft-deleted record.
    /// </summary>
    /// <param name="firstName">New first name. Required.</param>
    /// <param name="lastName">New last name. Required.</param>
    /// <param name="phone">New phone number. Required.</param>
    /// <param name="email">New email, or null to clear.</param>
    /// <param name="dateOfBirth">New date of birth, or null to clear.</param>
    /// <exception cref="DomainException">Thrown when the record is deleted or a required field is blank.</exception>
    public void UpdateDetails(string firstName, string lastName, string phone,
        string? email = null, DateTime? dateOfBirth = null)
    {
        if (IsDeleted)
            throw new DomainException("Cannot update a deleted customer record.");
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Phone number is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone.Trim();
        Email = NormaliseEmail(email);
        DateOfBirth = dateOfBirth?.Date;
    }

    /// <summary>
    /// Updates the staff notes for this customer (allergies, colour formula, preferences).
    /// </summary>
    /// <param name="notes">New notes text. Maximum 2000 characters. Pass null to clear.</param>
    /// <exception cref="DomainException">Thrown when notes exceed 2000 characters or record is deleted.</exception>
    public void UpdateNotes(string? notes)
    {
        if (IsDeleted)
            throw new DomainException("Cannot update a deleted customer record.");
        if (notes?.Length > 2000)
            throw new DomainException("Notes cannot exceed 2000 characters.");

        Notes = notes?.Trim();
    }

    /// <summary>
    /// Records the timestamp of the customer's most recent completed visit.
    /// Called by the booking completion flow, not directly by the user.
    /// </summary>
    /// <param name="visitedAt">UTC timestamp of the completed booking.</param>
    public void RecordVisit(DateTime visitedAt)
    {
        if (LastVisitAt == null || visitedAt > LastVisitAt)
            LastVisitAt = visitedAt;
    }

    /// <summary>
    /// Soft-deletes this customer record. The row stays in the database so
    /// historical bookings and sales are never orphaned.
    /// </summary>
    /// <exception cref="DomainException">Thrown when already deleted.</exception>
    public void SoftDelete()
    {
        if (IsDeleted)
            throw new DomainException("This customer record has already been deleted.");
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    // ── Computed properties ───────────────────────────────────────────

    /// <summary>Full display name, e.g. "Jane Smith".</summary>
    public string FullName => $"{FirstName} {LastName}";

    // ── Private helpers ───────────────────────────────────────────────

    private static string? NormaliseEmail(string? email)
        => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
}