using System.Text.Json;
using Salon.Application.DTOs;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Creates a new booking and writes a "Created" entry to the shared audit log.
/// </summary>
public class CreateBookingHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public CreateBookingHandler(
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
    /// Workflow: load service → check overlap → create booking → save → write audit → return DTO.
    /// </summary>
    /// <param name="command">Validated input from the controller.</param>
    /// <returns>The newly created booking as a DTO including its new database ID.</returns>
    /// <exception cref="NotFoundException">Thrown when the service ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when the slot is taken or date is in the past.</exception>
    public async Task<BookingDto> Handle(CreateBookingCommand command)
    {
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId)
            ?? throw new NotFoundException("Service", command.ServiceId);

        var endTime = command.StartTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));

        var overlap = await _bookingRepository.ExistsOverlappingBookingAsync(
            command.StaffId, command.BookingDate, command.StartTime, endTime);

        if (overlap)
            throw new DomainException("Staff is already booked for this time slot.");

        var booking = new Booking(
            command.CustomerId, command.StaffId, command.ServiceId,
            command.BookingDate, command.StartTime, endTime, service.BasePrice);

        booking.SetNotes(command.Notes);
        await _bookingRepository.AddAsync(booking);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Booking",
            entityId: booking.Id,
            action: "Created",
            description: $"Booking #{booking.Id} created by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
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