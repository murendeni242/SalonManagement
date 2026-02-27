using Salon.Domain.Common;
using Salon.Domain.Enums;

namespace Salon.Domain.Entities;

/// <summary>
/// Core booking aggregate. Every business rule for a booking lives here.
/// No outside layer can put the booking into an invalid state.
/// </summary>
public class Booking
{
    /// <summary>Database primary key.</summary>
    public int Id { get; private set; }

    /// <summary>Customer who made the appointment.</summary>
    public int CustomerId { get; private set; }

    /// <summary>Staff member assigned to perform the service.</summary>
    public int StaffId { get; private set; }

    /// <summary>Service being performed.</summary>
    public int ServiceId { get; private set; }

    /// <summary>Calendar date of the appointment.</summary>
    public DateTime BookingDate { get; private set; }

    /// <summary>Time of day the appointment starts, e.g. 09:30:00.</summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>Time the appointment ends. Set as StartTime + service duration.</summary>
    public TimeSpan EndTime { get; private set; }

    /// <summary>
    /// Price snapshotted from the service at booking time so future
    /// price changes do not affect historical records.
    /// </summary>
    public decimal TotalPrice { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public BookingStatus Status { get; private set; }

    /// <summary>Optional free-text notes. Maximum 500 characters.</summary>
    public string? Notes { get; private set; }

    /// <summary>True when the booking has been soft-deleted.</summary>
    public bool IsDeleted { get; private set; }

    /// <summary>UTC timestamp of the soft-delete, or null if not deleted.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Required by EF Core. Do not call directly.</summary>
    protected Booking() { }

    /// <summary>
    /// Creates a new booking in Pending state.
    /// </summary>
    /// <param name="customerId">ID of the customer.</param>
    /// <param name="staffId">ID of the assigned staff member.</param>
    /// <param name="serviceId">ID of the service being booked.</param>
    /// <param name="bookingDate">Appointment date. Must not be in the past.</param>
    /// <param name="startTime">Time the appointment starts.</param>
    /// <param name="endTime">Time the appointment ends (startTime + service duration).</param>
    /// <param name="totalPrice">Price snapshotted from the service.</param>
    /// <exception cref="DomainException">Thrown when bookingDate is earlier than today.</exception>
    public Booking(int customerId, int staffId, int serviceId,
        DateTime bookingDate, TimeSpan startTime, TimeSpan endTime, decimal totalPrice)
    {
        if (bookingDate.Date < DateTime.UtcNow.Date)
            throw new DomainException("Cannot create a booking in the past.");

        CustomerId = customerId;
        StaffId = staffId;
        ServiceId = serviceId;
        BookingDate = bookingDate;
        StartTime = startTime;
        EndTime = endTime;
        TotalPrice = totalPrice;
        Status = BookingStatus.Pending;
    }

    /// <summary>
    /// Updates scheduling fields. Only allowed when the booking is Pending.
    /// </summary>
    /// <exception cref="DomainException">Thrown when not Pending or new date is in the past.</exception>
    public void Update(int staffId, int serviceId, DateTime bookingDate,
        TimeSpan startTime, TimeSpan endTime, decimal totalPrice)
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Only Pending bookings can be updated.");

        if (bookingDate.Date < DateTime.UtcNow.Date)
            throw new DomainException("Cannot reschedule a booking to a past date.");

        StaffId = staffId;
        ServiceId = serviceId;
        BookingDate = bookingDate;
        StartTime = startTime;
        EndTime = endTime;
        TotalPrice = totalPrice;
    }

    /// <summary>Sets or clears the notes. Maximum 500 characters.</summary>
    /// <exception cref="DomainException">Thrown when notes exceed 500 characters.</exception>
    public void SetNotes(string? notes)
    {
        if (notes?.Length > 500)
            throw new DomainException("Notes cannot exceed 500 characters.");
        Notes = notes;
    }

    /// <summary>Transitions Pending → Confirmed.</summary>
    /// <exception cref="DomainException">Thrown when not Pending.</exception>
    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Only pending bookings can be confirmed.");
        Status = BookingStatus.Confirmed;
    }

    /// <summary>Transitions Confirmed → Completed.</summary>
    /// <exception cref="DomainException">Thrown when not Confirmed.</exception>
    public void Complete()
    {
        if (Status != BookingStatus.Confirmed)
            throw new DomainException("Booking must be confirmed first.");
        Status = BookingStatus.Completed;
    }

    /// <summary>Cancels the booking. Allowed from Pending or Confirmed state.</summary>
    /// <exception cref="DomainException">Thrown when already Completed or Cancelled.</exception>
    public void Cancel()
    {
        if (Status == BookingStatus.Completed)
            throw new DomainException("A Completed booking cannot be cancelled.");
        if (Status == BookingStatus.Cancelled)
            throw new DomainException("Booking is already cancelled.");
        Status = BookingStatus.Cancelled;
    }

    /// <summary>
    /// Soft-deletes the booking. The row is kept in the database for the audit trail
    /// but hidden from all normal queries via EF Core global query filter.
    /// </summary>
    /// <exception cref="DomainException">Thrown when already deleted.</exception>
    public void SoftDelete()
    {
        if (IsDeleted)
            throw new DomainException("This booking has already been deleted.");
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}