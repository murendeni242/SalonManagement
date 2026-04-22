
namespace Salon.Application.DTOs
{
    /// <summary>
    /// Provides an aggregated summary of commission data for a staff member.
    /// </summary>
    public class CommissionSummaryDto
    {
        /// <summary>
        /// Identifier of the staff member.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Full name of the staff member.
        /// </summary>
        public string StaffName { get; set; } = default!;

        /// <summary>
        /// Total commission amount earned (includes both paid and pending).
        /// </summary>
        public decimal TotalEarned { get; set; }

        /// <summary>
        /// Total commission amount that is still pending payment.
        /// </summary>
        public decimal TotalPending { get; set; }

        /// <summary>
        /// Total commission amount that has been paid.
        /// </summary>
        public decimal TotalPaid { get; set; }

        /// <summary>
        /// Total number of commission records for the staff member.
        /// </summary>
        public int TotalRecords { get; set; }
    }
}
