using Salon.Application.DTOs.Sales;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Sales;

/// <summary>
/// Returns a single sale record by primary key.
/// </summary>
public class GetSaleByIdHandler
{
    private readonly ISaleRepository _saleRepository;

    public GetSaleByIdHandler(ISaleRepository saleRepository)
        => _saleRepository = saleRepository;

    /// <summary>
    /// Returns the sale with the given <paramref name="id"/>, or null if not found.
    /// </summary>
    public async Task<SaleDto?> Handle(int id)
    {
        var s = await _saleRepository.GetByIdAsync(id);
        return s is null ? null : CreateSaleHandler.ToDto(s);
    }
}

/// <summary>
/// Returns the full payment history for a specific booking.
/// Includes all sale types: normal payments, refund records (negative amount), and voided entries.
/// Useful on the booking detail screen to show how much has been paid and what was refunded.
/// </summary>
public class GetSalesByBookingHandler
{
    private readonly ISaleRepository _saleRepository;

    public GetSalesByBookingHandler(ISaleRepository saleRepository)
        => _saleRepository = saleRepository;

    /// <summary>
    /// Returns all sale records for the given <paramref name="bookingId"/> ordered by PaidAt ascending.
    /// </summary>
    public async Task<IEnumerable<SaleDto>> Handle(int bookingId)
    {
        var sales = await _saleRepository.GetByBookingIdAsync(bookingId);
        return sales.Select(CreateSaleHandler.ToDto);
    }
}

/// <summary>
/// Returns the full audit history for a sale record, oldest first.
/// Uses the same shared IAuditLogRepository as bookings and services.
/// entityName = "Sale". Owner role only.
/// </summary>
public class GetSaleAuditLogsHandler
{
    private readonly IAuditLogRepository _auditLog;

    public GetSaleAuditLogsHandler(IAuditLogRepository auditLog)
        => _auditLog = auditLog;

    /// <summary>
    /// Returns all audit entries for the sale with the given <paramref name="saleId"/>,
    /// ordered by ChangedAt ascending.
    /// </summary>
    public async Task<IEnumerable<DTOs.AuditLogDto>> Handle(int saleId)
    {
        var logs = await _auditLog.GetByEntityAsync("Sale", saleId);
        return logs.Select(l => new DTOs.AuditLogDto
        {
            Id = l.Id,
            EntityName = l.EntityName,
            EntityId = l.EntityId,
            Action = l.Action,
            Description = l.Description,
            OldValues = l.OldValues,
            NewValues = l.NewValues,
            ChangedBy = l.ChangedBy,
            ChangedAt = l.ChangedAt
        });
    }
}