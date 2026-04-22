using Salon.Domain.Enums;

namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Command to create or update a commission rule for a staff member.
    /// </summary>
    public class UpsertCommissionRuleCommand
    {
        /// <summary>
        /// Identifier of the staff member the rule applies to.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Type of commission rule (e.g., Percentage, Fixed, Tiered).
        /// </summary>
        public CommissionType Type { get; set; }

        /// <summary>
        /// Commission rate or fixed amount.
        /// Used for Percentage and Fixed types; ignored for Tiered.
        /// </summary>
        public decimal RateOrAmount { get; set; }

        /// <summary>
        /// Collection of tiers used when the commission type is Tiered.
        /// Ignored for Percentage and Fixed types.
        /// </summary>
        public List<UpsertTierCommand> Tiers { get; set; } = new();
    }
}
