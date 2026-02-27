using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Sales;

/// <summary>
/// Voids an erroneous Paid sale entry. Owner role only.
///
/// A void is for data-entry mistakes — wrong amount typed, wrong booking linked.
/// It is NOT a refund. No money is returned to the customer.
/// The record stays in the database with Status = Voided and the reason in Notes.
/// </summary>
public class VoidSaleHandler
{
    private readonly ISaleRepository _saleRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public VoidSaleHandler(
        ISaleRepository saleRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _saleRepository = saleRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Voids the sale identified by <paramref name="command"/>.SaleId.
    /// </summary>
    /// <param name="command">Void details including the mandatory reason.</param>
    /// <exception cref="NotFoundException">Thrown when the sale ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when not Paid or reason is blank.</exception>
    public async Task Handle(VoidSaleCommand command)
    {
        var sale = await _saleRepository.GetByIdAsync(command.SaleId)
            ?? throw new NotFoundException("Sale", command.SaleId);

        var oldSnapshot = System.Text.Json.JsonSerializer.Serialize(new
        { Status = sale.Status.ToString(), sale.Notes });

        sale.Void(command.Reason);
        await _saleRepository.UpdateAsync(sale);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Sale",
            entityId: sale.Id,
            action: "Voided",
            description: $"Sale #{sale.Id} voided by {_currentUser.UserEmail}. Reason: {command.Reason}.",
            changedBy: _currentUser.UserEmail,
            oldValues: oldSnapshot,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            { Status = "Voided", Reason = command.Reason })));
    }
}