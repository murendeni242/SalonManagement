using Salon.Application.DTOs;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Returns a single booking by primary key, including soft-deleted ones
/// so admin users can inspect a deleted booking by ID.
/// </summary>
public class GetBookingByIdHandler
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByIdHandler(IBookingRepository bookingRepository)
        => _bookingRepository = bookingRepository;

    /// <summary>
    /// Returns the booking with the given <paramref name="id"/>, or null if not found.
    /// </summary>
    public async Task<BookingDto?> Handle(int id)
    {
        var b = await _bookingRepository.GetByIdAsync(id);
        if (b is null) return null;

        return new BookingDto
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
}