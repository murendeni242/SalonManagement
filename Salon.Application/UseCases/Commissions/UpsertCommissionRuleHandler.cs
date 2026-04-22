using Salon.Application.DTOs;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Creates or updates the commission rule for a staff member.
    /// For tiered rules, replaces the entire set of tiers.
    /// </summary>
    public class UpsertCommissionRuleHandler
    {
        private readonly ICommissionRuleRepository _ruleRepo;
        private readonly IStaffRepository _staffRepo;

        public UpsertCommissionRuleHandler(
            ICommissionRuleRepository ruleRepo,
            IStaffRepository staffRepo)
        {
            _ruleRepo = ruleRepo;
            _staffRepo = staffRepo;
        }

        public async Task<CommissionRuleDto> Handle(UpsertCommissionRuleCommand command)
        {
            var staff = await _staffRepo.GetByIdAsync(command.StaffId)
                ?? throw new NotFoundException("Staff", command.StaffId);

            var existing = await _ruleRepo.GetByStaffIdAsync(command.StaffId);

            if (existing is not null)
            {
                // Update existing rule
                existing.ChangeType(command.Type, command.RateOrAmount);

                if (command.Type == Domain.Enums.CommissionType.Tiered)
                {
                    existing.Tiers.Clear();
                    foreach (var tier in command.Tiers)
                        existing.Tiers.Add(new CommissionTier(
                            existing.Id,
                            tier.MinServices,
                            tier.MaxServices,
                            tier.Percentage));
                }

                await _ruleRepo.UpdateAsync(existing);
                return ToDto(existing, staff.FullName);
            }

            // Create new rule
            CommissionRule rule;

            if (command.Type == Domain.Enums.CommissionType.Tiered)
            {
                rule = new CommissionRule(command.StaffId);
                foreach (var tier in command.Tiers)
                    rule.Tiers.Add(new CommissionTier(
                        0,
                        tier.MinServices,
                        tier.MaxServices,
                        tier.Percentage));
            }
            else
            {
                rule = new CommissionRule(
                    command.StaffId, command.Type, command.RateOrAmount);
            }

            await _ruleRepo.AddAsync(rule);
            return ToDto(rule, staff.FullName);
        }

        private static CommissionRuleDto ToDto(CommissionRule r, string staffName) => new()
        {
            Id = r.Id,
            StaffId = r.StaffId,
            StaffName = staffName,
            Type = r.Type.ToString(),
            RateOrAmount = r.RateOrAmount,
            Tiers = r.Tiers.Select(t => new CommissionTierDto
            {
                Id = t.Id,
                MinServices = t.MinServices,
                MaxServices = t.MaxServices,
                Percentage = t.Percentage
            }).ToList()
        };
    }
}
