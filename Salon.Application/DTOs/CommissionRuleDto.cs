namespace Salon.Application.DTOs
{
    /// <summary>
    /// Defines the commission rule configuration for a staff member,
    /// including calculation type and optional tiered structure.
    /// </summary>
    public class CommissionRuleDto
    {
        /// <summary>
        /// Unique identifier for the commission rule.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the staff member this rule applies to.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Full name of the staff member.
        /// </summary>
        public string StaffName { get; set; } = default!;

        /// <summary>
        /// Type of commission rule (e.g., percentage-based, fixed, tiered).
        /// </summary>
        public string Type { get; set; } = default!;

        /// <summary>
        /// Commission rate or fixed amount applied when not using tiers.
        /// </summary>
        public decimal RateOrAmount { get; set; }

        /// <summary>
        /// Collection of tier definitions used when the commission rule is tiered.
        /// </summary>
        public List<CommissionTierDto> Tiers { get; set; } = new();
    }
}
