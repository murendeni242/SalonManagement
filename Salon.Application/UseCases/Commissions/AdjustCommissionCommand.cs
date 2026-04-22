namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Command to adjust an existing commission due to a refund or correction.
    /// </summary>
    public class AdjustCommissionCommand
    {
        /// <summary>
        /// Identifier of the related sale transaction.
        /// </summary>
        public int SaleId { get; set; }

        /// <summary>
        /// Original commissionable amount before adjustment.
        /// </summary>
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Amount that was refunded and should reduce the commission.
        /// </summary>
        public decimal RefundedAmount { get; set; }
    }
}
