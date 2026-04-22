using Salon.Application.DTOs;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Calculates and saves a commission record when a payment (Sale) is created.
    /// Called directly from CreateSaleHandler after saving the sale.
    ///
    /// If no commission rule is configured for the staff member, no commission
    /// is created — this is not an error, just means the staff member is not
    /// on commission.
    /// </summary>
    public class CalculateCommissionHandler
    {
        private readonly ICommissionRepository _commissionRepo;
        private readonly ICommissionRuleRepository _ruleRepo;
        private readonly ICommissionStrategyResolver _resolver;

        public CalculateCommissionHandler(
            ICommissionRepository commissionRepo,
            ICommissionRuleRepository ruleRepo,
            ICommissionStrategyResolver resolver)
        {
            _commissionRepo = commissionRepo;
            _ruleRepo = ruleRepo;
            _resolver = resolver;
        }

        /// <summary>
        /// Workflow: load rule → resolve strategy → calculate → save commission.
        /// Returns null when no rule is configured for the staff member.
        /// </summary>
        public async Task<CommissionDto?> Handle(CalculateCommissionCommand command)
        {
            // No rule = no commission — not an error
            var rule = await _ruleRepo.GetByStaffIdAsync(command.StaffId);
            if (rule is null) return null;

            // For tiered strategy — get completed services this month
            var completedThisMonth = await _commissionRepo
                .GetCompletedServicesThisMonthAsync(command.StaffId);

            var strategy = _resolver.Resolve(rule.Type);
            var amount = strategy.Calculate(
                command.PaymentAmount, rule, completedThisMonth);

            var commission = new Commission(
                saleId: command.SaleId,
                staffId: command.StaffId,
                amount: amount,
                rateApplied: rule.RateOrAmount,
                type: rule.Type);

            await _commissionRepo.AddAsync(commission);

            return ToDto(commission);
        }

        internal static CommissionDto ToDto(Commission c) => new()
        {
            Id = c.Id,
            SaleId = c.SaleId,
            StaffId = c.StaffId,
            GrossAmount = c.GrossAmount,
            Amount = c.Amount,
            RateApplied = c.RateApplied,
            Type = c.Type.ToString(),
            Status = c.Status.ToString(),
            PaidAt = c.PaidAt,
            PaidBy = c.PaidBy,
            CreatedAt = c.CreatedAt
        };
    }
}
