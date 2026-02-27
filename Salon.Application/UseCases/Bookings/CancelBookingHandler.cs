using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Cancels a Pending or Confirmed booking and writes a "Cancelled" audit entry.
/// Only Owner or Reception roles may call this.
/// </summary>
public class CancelBookingHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public CancelBookingHandler(
        IBookingRepository bookingRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _bookingRepository = bookingRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Cancels the booking with the given <paramref name="bookingId"/>.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the booking does not exist.</exception>
    /// <exception cref="DomainException">Thrown when already Completed or Cancelled.</exception>
    public async Task Handle(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking", bookingId);

        booking.Cancel();
        await _bookingRepository.UpdateAsync(booking);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Booking",
            entityId: booking.Id,
            action: "Cancelled",
            description: $"Booking #{booking.Id} cancelled by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail));
    }
}