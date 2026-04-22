namespace Salon.Application.DTOs
{
    /// <summary>
    /// Represents a tier within a tiered commission structure,
    /// defining thresholds and the applicable commission percentage.
    /// </summary>
    public class CommissionTierDto
    {
        /// <summary>
        /// Unique identifier for the commission tier.
        /// </summary>
        public int Id { get; set; }

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
        /// Commission percentage applied when this tier is reached.
        /// </summary>
        public decimal Percentage { get; set; }
    }
}
