namespace Salon.Application.DTOs
{
    /// <summary>
    /// Represents commission details for a staff member based on a sale.
    /// </summary>
    public class CommissionDto
    {
        /// <summary>
        /// Unique identifier for the commission record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the related sale transaction.
        /// </summary>
        public int SaleId { get; set; }

        /// <summary>
        /// Identifier of the staff member who earned the commission.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Full name of the staff member.
        /// </summary>
        public string StaffName { get; set; } = default!;

        /// <summary>
        /// Total gross amount from the sale before commission is applied.
        /// </summary>
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// Final commission amount earned by the staff member.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Commission rate applied (e.g., percentage or fixed rate).
        /// </summary>
        public decimal RateApplied { get; set; }

        /// <summary>
        /// Type of commission (e.g., percentage-based, fixed).
        /// </summary>
        public string Type { get; set; } = default!;

        /// <summary>
        /// Current status of the commission (e.g., pending, paid).
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// Date and time when the commission was paid, if applicable.
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Identifier or name of the user who processed the payment.
        /// </summary>
        public string? PaidBy { get; set; }

        /// <summary>
        /// Date and time when the commission record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
