using Salon.Application.DTOs;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Marks one or more pending commissions as paid for a staff member.
    /// If CommissionIds is empty, marks ALL pending commissions for the staff member.
    /// </summary>
    public class MarkCommissionPaidHandler
    {
        private readonly ICommissionRepository _commissionRepo;
        private readonly ICurrentUserService _currentUser;

        public MarkCommissionPaidHandler(
            ICommissionRepository commissionRepo,
            ICurrentUserService currentUser)
        {
            _commissionRepo = commissionRepo;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<CommissionDto>> Handle(MarkCommissionPaidCommand command)
        {
            IEnumerable<Commission> toMark;

            if (command.CommissionIds.Any())
            {
                var all = await _commissionRepo.GetPendingByStaffIdAsync(command.StaffId);
                toMark = all.Where(c => command.CommissionIds.Contains(c.Id));
            }
            else
            {
                toMark = await _commissionRepo.GetPendingByStaffIdAsync(command.StaffId);
            }

            var results = new List<CommissionDto>();

            foreach (var commission in toMark)
            {
                commission.MarkPaid(_currentUser.UserEmail);
                await _commissionRepo.UpdateAsync(commission);
                results.Add(CalculateCommissionHandler.ToDto(commission));
            }

            return results;
        }
    }
}
