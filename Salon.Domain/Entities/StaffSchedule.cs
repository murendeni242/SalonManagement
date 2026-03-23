using Salon.Domain.Common;

namespace Salon.Domain.Entities;

/// <summary>
/// Defines the working hours for a staff member on a specific day of the week.
/// </summary>
public class StaffSchedule
{
    // ── Identity ──────────────────────────────────────────────────────

    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    // ── Foreign key ───────────────────────────────────────────────────

    /// <summary>The staff member this schedule row belongs to.</summary>
    public int StaffId { get; private set; }

    /// <summary>Navigation property — loaded when needed.</summary>
    public Staff Staff { get; private set; } = default!;

    // ── Schedule ──────────────────────────────────────────────────────

    /// <summary>
    /// Day of the week this row applies to.
    /// Uses .NET DayOfWeek: Sunday=0, Monday=1 … Saturday=6.
    /// </summary>
    public DayOfWeek DayOfWeek { get; private set; }

    /// <summary>
    /// The time the staff member starts work on this day.
    /// Stored as TimeSpan — e.g. 09:00 = new TimeSpan(9, 0, 0).
    /// </summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>
    /// The time the staff member finishes work on this day.
    /// Must be later than StartTime.
    /// </summary>
    public TimeSpan EndTime { get; private set; }

    // ── EF Core ───────────────────────────────────────────────────────

    /// <summary>Required by EF Core. Do not call directly.</summary>
    protected StaffSchedule() { }

    // ── Constructor ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a new working hours row for a staff member.
    /// </summary>
    /// <param name="staffId">FK to the Staff record.</param>
    /// <param name="dayOfWeek">Day this row applies to.</param>
    /// <param name="startTime">Start of working hours.</param>
    /// <param name="endTime">End of working hours. Must be after startTime.</param>
    /// <exception cref="DomainException">
    /// Thrown when endTime is not later than startTime.
    /// </exception>
    public StaffSchedule(int staffId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        if (endTime <= startTime)
            throw new DomainException("End time must be later than start time.");

        StaffId = staffId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    // ── State-change methods ──────────────────────────────────────────

    /// <summary>
    /// Updates the working hours for this schedule row.
    /// Called by UpsertStaffScheduleHandler when the row already exists.
    /// </summary>
    /// <param name="startTime">New start of working hours.</param>
    /// <param name="endTime">New end of working hours. Must be after startTime.</param>
    /// <exception cref="DomainException">
    /// Thrown when endTime is not later than startTime.
    /// </exception>
    public void UpdateHours(TimeSpan startTime, TimeSpan endTime)
    {
        if (endTime <= startTime)
            throw new DomainException("End time must be later than start time.");

        StartTime = startTime;
        EndTime = endTime;
    }

    // ── Computed properties ───────────────────────────────────────────

    /// <summary>
    /// Returns true when the given time window falls entirely within this staff member's working hours.
    /// </summary>
    /// <param name="bookingStart">Proposed booking start time.</param>
    /// <param name="bookingEnd">Proposed booking end time.</param>
    public bool CoversWindow(TimeSpan bookingStart, TimeSpan bookingEnd)
    {
        return bookingStart >= StartTime && bookingEnd <= EndTime;
    }
}