using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces
{
    public interface ICommissionRuleRepository
    {
        /// <summary>Saves a new commission rule.</summary>
        Task AddAsync(CommissionRule rule);

        /// <summary>Saves changes to an existing commission rule.</summary>
        Task UpdateAsync(CommissionRule rule);

        /// <summary>Hard deletes a commission rule.</summary>
        Task DeleteAsync(CommissionRule rule);

        /// <summary>
        /// Returns the commission rule for a specific staff member,
        /// including their tiers, or null if no rule is configured.
        /// </summary>
        Task<CommissionRule?> GetByStaffIdAsync(int staffId);

        /// <summary>Returns all commission rules with their tiers.</summary>
        Task<IEnumerable<CommissionRule>> GetAllAsync();
    }
}
