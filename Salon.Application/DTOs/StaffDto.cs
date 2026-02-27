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

/// <summary>
/// A single appointment slot on a staff member's daily schedule.
/// Returned by GET /api/staff/{id}/schedule?date=yyyy-MM-dd.
/// </summary>
public class StaffScheduleItemDto
{
    /// <summary>Booking primary key.</summary>
    public int BookingId { get; set; }

    /// <summary>ID of the customer for this appointment.</summary>
    public int CustomerId { get; set; }

    /// <summary>ID of the service being performed.</summary>
    public int ServiceId { get; set; }

    /// <summary>Calendar date of the appointment.</summary>
    public DateTime BookingDate { get; set; }

    /// <summary>Time the appointment starts.</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>Time the appointment ends.</summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>Current booking status: Pending | Confirmed | Completed | Cancelled.</summary>
    public string Status { get; set; } = default!;

    /// <summary>Optional notes on the booking.</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Full daily schedule for a staff member returned by the schedule endpoint.
/// </summary>
public class StaffScheduleDto
{
    /// <summary>Staff member the schedule belongs to.</summary>
    public int StaffId { get; set; }

    /// <summary>Full display name for the header.</summary>
    public string StaffName { get; set; } = default!;

    /// <summary>The date this schedule covers.</summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Appointments for this day, ordered by StartTime ascending.
    /// Empty list means the staff member has no bookings for this date.
    /// </summary>
    public List<StaffScheduleItemDto> Appointments { get; set; } = new();

    /// <summary>Total number of appointments for the day.</summary>
    public int TotalAppointments => Appointments.Count;
}