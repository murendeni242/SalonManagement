using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Soft-deletes a booking and writes a "SoftDeleted" audit entry.
/// The row stays in the database — the audit trail is never broken.
/// Only the Owner role may call this.
/// </summary>
public class DeleteBookingHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public DeleteBookingHandler(
        IBookingRepository bookingRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _bookingRepository = bookingRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Soft-deletes the booking with the given <paramref name="bookingId"/>.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the booking does not exist.</exception>
    /// <exception cref="DomainException">Thrown when already deleted.</exception>
    public async Task Handle(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking", bookingId);

        booking.SoftDelete();
        await _bookingRepository.UpdateAsync(booking);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Booking",
            entityId: booking.Id,
            action: "SoftDeleted",
            description: $"Booking #{booking.Id} deleted by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail));
    }
}