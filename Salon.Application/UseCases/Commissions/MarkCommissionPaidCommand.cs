namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Command to mark one or more commissions as paid for a staff member.
    /// </summary>
    public class MarkCommissionPaidCommand
    {
        /// <summary>
        /// Identifier of the staff member whose commissions are being marked as paid.
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Optional list of specific commission IDs to mark as paid.
        /// If empty, all pending commissions for this staff member are marked paid.
        /// </summary>
        public List<int> CommissionIds { get; set; } = new();
    }
}
