namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Command to define or update a single tier in a tiered commission rule.
    /// </summary>
    public class UpsertTierCommand
    {
        /// <summary>
        /// Minimum number of services required to qualify for this tier.
        /// </summary>
        public int MinServices { get; set; }

        /// <summary>
        /// Maximum number of services allowed for this tier.
        /// If null, the tier has no upper limit.
        /// </summary>
        public int? MaxServices { get; set; }

        /// <summary>
        /// Commission percentage applied for this tier.
        /// </summary>
        public decimal Percentage { get; set; }
    }
}
