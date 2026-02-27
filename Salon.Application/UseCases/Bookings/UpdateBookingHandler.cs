using System.Text.Json;
using Salon.Application.DTOs;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Updates a Pending booking and writes an "Updated" entry to the shared audit log
/// with a before/after JSON snapshot so the exact change is always traceable.
/// </summary>
public class UpdateBookingHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public UpdateBookingHandler(
        IBookingRepository bookingRepository,
        IServiceRepository serviceRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Workflow: load booking → snapshot old state → load new service → check overlap
    /// → apply changes → save → write audit (old + new) → return DTO.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the booking or service ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when not Pending, slot conflicts, or date is in the past.</exception>
    public async Task<BookingDto> Handle(UpdateBookingCommand command)
    {
        var booking = await _bookingRepository.GetByIdAsync(command.Id)
            ?? throw new NotFoundException("Booking", command.Id);

        var oldSnapshot = Snapshot(booking);

        var service = await _serviceRepository.GetByIdAsync(command.ServiceId)
            ?? throw new NotFoundException("Service", command.ServiceId);

        var endTime = command.StartTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));

        // Exclude this booking from the overlap check so it doesn't conflict with its own slot
        var overlap = await _bookingRepository.ExistsOverlappingBookingAsync(
            command.StaffId, command.BookingDate, command.StartTime, endTime,
            excludeBookingId: command.Id);

        if (overlap)
            throw new DomainException("The new time slot conflicts with an existing booking.");

        booking.Update(command.StaffId, command.ServiceId,
            command.BookingDate, command.StartTime, endTime, service.BasePrice);
        booking.SetNotes(command.Notes);

        await _bookingRepository.UpdateAsync(booking);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Booking",
            entityId: booking.Id,
            action: "Updated",
            description: $"Booking #{booking.Id} updated by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            oldValues: oldSnapshot,
            newValues: Snapshot(booking)));

        return ToDto(booking);
    }

    private static string Snapshot(Booking b) => JsonSerializer.Serialize(new
    { b.StaffId, b.ServiceId, b.BookingDate, b.StartTime, b.EndTime, b.TotalPrice, b.Notes });

    private static BookingDto ToDto(Booking b) => new()
    {
        Id = b.Id,
        CustomerId = b.CustomerId,
        StaffId = b.StaffId,
        ServiceId = b.ServiceId,
        BookingDate = b.BookingDate,
        StartTime = b.StartTime,
        EndTime = b.EndTime,
        TotalPrice = b.TotalPrice,
        Status = b.Status.ToString(),
        Notes = b.Notes,
        IsDeleted = b.IsDeleted,
        DeletedAt = b.DeletedAt
    };
}