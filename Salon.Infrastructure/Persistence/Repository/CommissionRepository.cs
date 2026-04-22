using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;

namespace Salon.Infrastructure.Persistence.Repository
{
    public class CommissionRepository : ICommissionRepository
    {
        private readonly SalonDbContext _context;

        public CommissionRepository(SalonDbContext context) => _context = context;

        public async Task AddAsync(Commission commission)
        {
            _context.Commissions.Add(commission);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Commission commission)
        {
            _context.Commissions.Update(commission);
            await _context.SaveChangesAsync();
        }

        public async Task<Commission?> GetByIdAsync(int id)
        {
            return await _context.Commissions
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Commission?> GetBySaleIdAsync(int saleId)
        {
            return await _context.Commissions
                .FirstOrDefaultAsync(c => c.SaleId == saleId);
        }

        public async Task<IEnumerable<Commission>> GetByStaffIdAsync(int staffId, DateTime from, DateTime to)
        {
            return await _context.Commissions
                .AsNoTracking()
                .Where(c =>
                    c.StaffId == staffId &&
                    c.CreatedAt >= from &&
                    c.CreatedAt <= to
                )
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Commission>> GetPendingByStaffIdAsync(int staffId)
        {
            return await _context.Commissions
                .AsNoTracking()
                .Where(c =>
                    c.StaffId == staffId &&
                    c.Status == CommissionStatus.Pending
                )
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalByStaffIdAsync(int staffId, DateTime from, DateTime to)
        {
            return await _context.Commissions
                .Where(c =>
                    c.StaffId == staffId &&
                    c.CreatedAt >= from &&
                    c.CreatedAt <= to &&
                    c.Status != CommissionStatus.Reversed
                )
                .SumAsync(c => c.Amount);
        }

        /// <summary>
        /// Counts completed bookings for a staff member in the current calendar month.
        /// Used by TieredCommissionStrategy.
        /// </summary>
        public async Task<int> GetCompletedServicesThisMonthAsync(int staffId)
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            return await _context.Bookings
                .Where(b => b.StaffId == staffId
                         && b.Status == BookingStatus.Completed
                         && b.BookingDate >= monthStart
                         && b.BookingDate < monthEnd)
                .CountAsync();
        }
    }
}
