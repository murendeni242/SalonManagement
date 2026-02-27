using Salon.Application.DTOs.Sales;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Sales;

/// <summary>
/// Issues a refund against an existing Paid sale.
///
/// How it works:
///   1. Load the original sale.
///   2. Call sale.Refund() — marks the original as Refunded and returns
///      a NEW Sale record with a negative AmountPaid.
///   3. Save BOTH the updated original and the new refund record.
///   4. Write a "Refunded" audit entry.
///
/// The original payment record is NEVER deleted.
/// Both records remain in the database for financial reconciliation.
/// Owner role only.
/// </summary>
public class RefundSaleHandler
{
    private readonly ISaleRepository _saleRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public RefundSaleHandler(
        ISaleRepository saleRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _saleRepository = saleRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Processes the refund and returns the new refund sale record as a DTO.
    /// </summary>
    /// <param name="command">Refund details from the controller.</param>
    /// <returns>The new refund sale record as a DTO (negative AmountPaid).</returns>
    /// <exception cref="NotFoundException">Thrown when the sale ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when not Paid, amount is invalid, or exceeds original.</exception>
    public async Task<SaleDto> Handle(RefundSaleCommand command)
    {
        var originalSale = await _saleRepository.GetByIdAsync(command.SaleId)
            ?? throw new NotFoundException("Sale", command.SaleId);

        // Refund() marks original as Refunded and returns the new negative-amount record
        var refundRecord = originalSale.Refund(
            command.RefundAmount,
            command.ProcessedByStaffId,
            command.Notes);

        // Save the updated original (Status is now Refunded)
        await _saleRepository.UpdateAsync(originalSale);

        // Save the new refund record (negative AmountPaid)
        await _saleRepository.AddAsync(refundRecord);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Sale",
            entityId: originalSale.Id,
            action: "Refunded",
            description: $"Refund of {command.RefundAmount:C} issued against Sale #{originalSale.Id} by {_currentUser.UserEmail}. Refund record ID: {refundRecord.Id}.",
            changedBy: _currentUser.UserEmail,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                RefundAmount = command.RefundAmount,
                RefundSaleId = refundRecord.Id,
                command.Notes
            })));

        return CreateSaleHandler.ToDto(refundRecord);
    }
}