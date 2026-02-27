using Salon.Application.DTOs.Sales;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Sales;

/// <summary>
/// Records a new payment against a booking and writes a "Created" audit entry.
///
/// A booking can have more than one sale — e.g. a deposit followed by the balance.
/// Both are valid and link to the same BookingId.
/// </summary>
public class CreateSaleHandler
{
    private readonly ISaleRepository _saleRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        IBookingRepository bookingRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _saleRepository = saleRepository;
        _bookingRepository = bookingRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Workflow: validate booking exists and is payable → create sale → save → write audit → return DTO.
    /// </summary>
    /// <param name="command">Validated input from the controller.</param>
    /// <returns>The newly created sale as a DTO.</returns>
    /// <exception cref="NotFoundException">Thrown when the booking ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when the booking is Cancelled or a payment rule is violated.</exception>
    public async Task<SaleDto> Handle(CreateSaleCommand command)
    {
        // Booking must exist and not be Cancelled before we accept payment
        var booking = await _bookingRepository.GetByIdAsync(command.BookingId)
            ?? throw new NotFoundException("Booking", command.BookingId);

        if (booking.Status == BookingStatus.Cancelled)
            throw new DomainException("Cannot record a payment against a Cancelled booking.");

        var sale = new Sale(
            command.BookingId,
            command.AmountPaid,
            command.PaymentMethod,
            command.ProcessedByStaffId,
            command.Notes);

        await _saleRepository.AddAsync(sale);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Sale",
            entityId: sale.Id,
            action: "Created",
            description: $"Payment of {sale.AmountPaid:C} via {sale.PaymentMethod} recorded for Booking #{sale.BookingId} by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            newValues: Snapshot(sale)));

        return ToDto(sale);
    }

    internal static string Snapshot(Sale s) =>
        System.Text.Json.JsonSerializer.Serialize(new
        { s.BookingId, s.AmountPaid, s.PaymentMethod, Status = s.Status.ToString(), s.ProcessedByStaffId, s.Notes });

    internal static SaleDto ToDto(Sale s) => new()
    {
        Id = s.Id,
        BookingId = s.BookingId,
        AmountPaid = s.AmountPaid,
        PaymentMethod = s.PaymentMethod,
        Status = s.Status.ToString(),
        PaidAt = s.PaidAt,
        ProcessedByStaffId = s.ProcessedByStaffId,
        Notes = s.Notes,
        OriginalSaleId = s.OriginalSaleId
    };
}