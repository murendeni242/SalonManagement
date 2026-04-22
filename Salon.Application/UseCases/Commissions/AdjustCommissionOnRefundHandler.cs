using Salon.Application.DTOs;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Adjusts an existing commission record when a sale is refunded.
    /// Called directly from the refund handler after processing the refund.
    /// </summary>
    public class AdjustCommissionOnRefundHandler
    {
        private readonly ICommissionRepository _commissionRepo;

        public AdjustCommissionOnRefundHandler(ICommissionRepository commissionRepo)
        { 
            _commissionRepo = commissionRepo; 
        }

        /// <summary>
        /// Returns null when no commission record exists for the sale
        /// (staff member was not on commission).
        /// </summary>
        public async Task<CommissionDto?> Handle(AdjustCommissionCommand command)
        {
            var commission = await _commissionRepo.GetBySaleIdAsync(command.SaleId);
            if (commission is null) return null;

            // Prevent division by zero
            if (command.OriginalAmount <= 0)
                return CalculateCommissionHandler.ToDto(commission);

            var remainingRatio = (command.OriginalAmount - command.RefundedAmount) / command.OriginalAmount;

            var newAmount = Math.Max(0, Math.Round(commission.GrossAmount * remainingRatio, 2));

            commission.AdjustForRefund(newAmount);
            await _commissionRepo.UpdateAsync(commission);

            return CalculateCommissionHandler.ToDto(commission);
        }
    }
}
