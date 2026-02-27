using Salon.Application.DTOs;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Bookings;

/// <summary>
/// Returns a paginated list of non-deleted bookings.
/// Pagination runs in SQL via GetPagedAsync — no full table load into memory.
/// </summary>
public class GetBookingsHandler
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingsHandler(IBookingRepository bookingRepository)
        => _bookingRepository = bookingRepository;

    /// <summary>
    /// Returns a page of bookings ordered by BookingDate descending.
    /// </summary>
    /// <param name="skip">Records to skip. Defaults to 0.</param>
    /// <param name="take">Max records to return. Defaults to 50.</param>
    public async Task<IEnumerable<BookingDto>> Handle(int skip = 0, int take = 50)
    {
        var bookings = await _bookingRepository.GetPagedAsync(skip, take);
        return bookings.Select(ToDto);
    }

    private static BookingDto ToDto(Domain.Entities.Booking b) => new()
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