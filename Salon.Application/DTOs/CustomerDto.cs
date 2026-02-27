namespace Salon.Application.DTOs.Customers;

/// <summary>
/// Standard read model for a customer returned to API consumers.
/// Used in lists and basic lookups.
/// </summary>
public class CustomerDto
{
    /// <summary>Database primary key.</summary>
    public int Id { get; set; }

    /// <summary>First name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Full display name, e.g. "Jane Smith".</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Primary contact phone number.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Optional email address.</summary>
    public string? Email { get; set; }

    /// <summary>Optional date of birth.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Staff notes: allergies, colour formulas, preferences.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>UTC timestamp of the customer's most recent completed visit.</summary>
    public DateTime? LastVisitAt { get; set; }

    /// <summary>True when this record has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>UTC timestamp of the soft-delete, or null if still active.</summary>
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Extended profile returned by GET /api/customers/{id}/profile.
/// Includes the customer's booking history summary and spend totals —
/// everything the Owner or reception needs on the customer detail screen.
/// </summary>
public class CustomerProfileDto
{
    /// <summary>Core customer details.</summary>
    public CustomerDto Customer { get; set; } = new();

    /// <summary>Total number of completed appointments.</summary>
    public int TotalVisits { get; set; }

    /// <summary>Total amount spent across all Paid sales (excluding refunds).</summary>
    public decimal TotalSpent { get; set; }

    /// <summary>UTC timestamp of the most recent completed booking.</summary>
    public DateTime? LastVisitAt { get; set; }

    /// <summary>
    /// Number of days since the last visit. Null if the customer has never visited.
    /// Used to identify lapsed customers (e.g. not visited in 90+ days).
    /// </summary>
    public int? DaysSinceLastVisit { get; set; }

    /// <summary>
    /// The 5 most recent bookings for this customer, newest first.
    /// Shown on the customer detail screen so staff can see visit history at a glance.
    /// </summary>
    public List<CustomerBookingHistoryDto> RecentBookings { get; set; } = new();
}

/// <summary>
/// A single booking shown in a customer's visit history.
/// </summary>
public class CustomerBookingHistoryDto
{
    /// <summary>Booking primary key.</summary>
    public int BookingId { get; set; }

    /// <summary>Date of the appointment.</summary>
    public DateTime BookingDate { get; set; }

    /// <summary>Time the appointment started.</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>ID of the service performed.</summary>
    public int ServiceId { get; set; }

    /// <summary>ID of the staff member who performed the service.</summary>
    public int StaffId { get; set; }

    /// <summary>Amount charged for this booking.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Booking status: Pending | Confirmed | Completed | Cancelled.</summary>
    public string Status { get; set; } = string.Empty;
}