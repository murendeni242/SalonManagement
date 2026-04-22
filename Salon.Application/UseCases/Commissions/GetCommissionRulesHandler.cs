using Salon.Application.DTOs;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Returns all commission rules — used by the Commission Settings page.
    /// </summary>
    public class GetCommissionRulesHandler
    {
        private readonly ICommissionRuleRepository _ruleRepo;
        private readonly IStaffRepository _staffRepo;

        public GetCommissionRulesHandler(
            ICommissionRuleRepository ruleRepo,
            IStaffRepository staffRepo)
        {
            _ruleRepo = ruleRepo;
            _staffRepo = staffRepo;
        }

        public async Task<IEnumerable<CommissionRuleDto>> Handle()
        {
            var rules = (await _ruleRepo.GetAllAsync()).ToList();
            var staff = (await _staffRepo.GetAllAsync()).ToList();

            return rules.Select(r => new CommissionRuleDto
            {
                Id = r.Id,
                StaffId = r.StaffId,
                StaffName = staff.FirstOrDefault(s => s.Id == r.StaffId)?.FullName ?? "Unknown",
                Type = r.Type.ToString(),
                RateOrAmount = r.RateOrAmount,
                Tiers = r.Tiers.Select(t => new CommissionTierDto
                {
                    Id = t.Id,
                    MinServices = t.MinServices,
                    MaxServices = t.MaxServices,
                    Percentage = t.Percentage
                }).ToList()
            });
        }
    }
}
