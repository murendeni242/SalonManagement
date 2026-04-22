using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Infrastructure.Persistence.Repository
{
    public class CommissionRuleRepository : ICommissionRuleRepository
    {
        private readonly SalonDbContext _context;

        public CommissionRuleRepository(SalonDbContext context) => _context = context;

        public async Task AddAsync(CommissionRule rule)
        {
            _context.CommissionRules.Add(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CommissionRule rule)
        {
            _context.CommissionRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CommissionRule rule)
        {
            _context.CommissionRules.Remove(rule);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the rule with tiers eagerly loaded — needed by TieredCommissionStrategy.
        /// </summary>
        public async Task<CommissionRule?> GetByStaffIdAsync(int staffId)
        {
            return await _context.CommissionRules
                .Include(r => r.Tiers)
                .FirstOrDefaultAsync(r => r.StaffId == staffId);
        }

        public async Task<IEnumerable<CommissionRule>> GetAllAsync()
        {
            return await _context.CommissionRules
                .Include(r => r.Tiers)
                .AsNoTracking()
                .OrderBy(r => r.StaffId)
                .ToListAsync();
        }
    }
}
