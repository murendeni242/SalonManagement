namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Command to calculate commission for a staff member based on a sale.
    /// </summary>
    public class CalculateCommissionCommand
    {
        /// <summary>
        /// Identifier of the related sale transaction.
        /// </summary>
        public int SaleId { get; set; }

        /// <summary>
        /// Identifier of the staff member earning the commission.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Amount paid for the sale used to calculate the commission.
        /// </summary>
        public decimal PaymentAmount { get; set; }
    }
}
