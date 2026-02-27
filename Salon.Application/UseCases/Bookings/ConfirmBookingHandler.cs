using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Confirms a Pending booking (Pending → Confirmed) and writes a "Confirmed" audit entry.
/// Only Owner or Reception roles may call this.
/// </summary>
public class ConfirmBookingHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public ConfirmBookingHandler(
        IBookingRepository bookingRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _bookingRepository = bookingRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Confirms the booking with the given <paramref name="bookingId"/>.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the booking does not exist.</exception>
    /// <exception cref="DomainException">Thrown when the booking is not Pending.</exception>
    public async Task Handle(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new NotFoundException("Booking", bookingId);

        booking.Confirm();
        await _bookingRepository.UpdateAsync(booking);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Booking",
            entityId: booking.Id,
            action: "Confirmed",
            description: $"Booking #{booking.Id} confirmed by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail));
    }
}