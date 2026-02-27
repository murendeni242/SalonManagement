namespace Salon.Application.DTOs;

/// <summary>
/// Read model returned to API consumers for all booking queries.
/// All write operations use separate Command objects.
/// </summary>
public class BookingDto
{
    /// <summary>Database primary key.</summary>
    public int Id { get; set; }

    /// <summary>ID of the customer who made the booking.</summary>
    public int CustomerId { get; set; }

    /// <summary>ID of the assigned staff member.</summary>
    public int StaffId { get; set; }

    /// <summary>ID of the service being performed.</summary>
    public int ServiceId { get; set; }

    /// <summary>Calendar date of the appointment.</summary>
    public DateTime BookingDate { get; set; }

    /// <summary>Time of day the appointment starts.</summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>Time of day the appointment ends (StartTime + service duration).</summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>Price snapshotted from the service at booking time.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Current status: Pending | Confirmed | Completed | Cancelled.</summary>
    public string Status { get; set; } = default!;

    /// <summary>Optional notes attached to the booking.</summary>
    public string? Notes { get; set; }

    /// <summary>True when this booking has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>UTC timestamp of the soft-delete, or null if not deleted.</summary>
    public DateTime? DeletedAt { get; set; }
}