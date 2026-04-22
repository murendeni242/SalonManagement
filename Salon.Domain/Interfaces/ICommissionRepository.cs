using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces
{
    public interface ICommissionRepository
    {
        /// <summary>Saves a new commission record.</summary>
        Task AddAsync(Commission commission);

        /// <summary>Saves changes to an existing commission record.</summary>
        Task UpdateAsync(Commission commission);

        /// <summary>Returns a commission by primary key, or null if not found.</summary>
        Task<Commission?> GetByIdAsync(int id);

        /// <summary>
        /// Returns the commission record for a specific sale, or null if not yet calculated.
        /// </summary>
        Task<Commission?> GetBySaleIdAsync(int saleId);

        /// <summary>
        /// Returns all commissions for a staff member within a date range.
        /// Used for the staff commission report and payout workflow.
        /// </summary>
        Task<IEnumerable<Commission>> GetByStaffIdAsync(int staffId, DateTime from, DateTime to);

        /// <summary>
        /// Returns all pending commissions for a staff member.
        /// Used by the mark-as-paid workflow.
        /// </summary>
        Task<IEnumerable<Commission>> GetPendingByStaffIdAsync(int staffId);

        /// <summary>
        /// Returns the total commission summary for a staff member in a date range.
        /// Used for the commission dashboard.
        /// </summary>
        Task<decimal> GetTotalByStaffIdAsync(int staffId, DateTime from, DateTime to);

        /// <summary>
        /// Returns the count of completed bookings for a staff member
        /// in the current calendar month.
        /// Used by TieredCommissionStrategy to determine which tier applies.
        /// </summary>
        Task<int> GetCompletedServicesThisMonthAsync(int staffId);
    }
}
