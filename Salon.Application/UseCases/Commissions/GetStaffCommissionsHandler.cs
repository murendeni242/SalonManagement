using Salon.Application.DTOs;
using Salon.Domain.Common;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Commissions
{
    /// <summary>
    /// Returns commissions for a specific staff member within a date range.
    /// Used by the staff commission report page.
    /// </summary>
    public class GetStaffCommissionsHandler
    {
        private readonly ICommissionRepository _commissionRepo;
        private readonly IStaffRepository _staffRepo;

        public GetStaffCommissionsHandler(
            ICommissionRepository commissionRepo,
            IStaffRepository staffRepo)
        {
            _commissionRepo = commissionRepo;
            _staffRepo = staffRepo;
        }

        public async Task<CommissionSummaryDto> Handle(int staffId, DateTime from, DateTime to)
        {
            var staff = await _staffRepo.GetByIdAsync(staffId)
                ?? throw new NotFoundException("Staff", staffId);

            var commissions = (await _commissionRepo
                .GetByStaffIdAsync(staffId, from, to)).ToList();

            return new CommissionSummaryDto
            {
                StaffId = staffId,
                StaffName = staff.FullName,
                TotalEarned = commissions.Sum(c => c.Amount),
                TotalPending = commissions
                    .Where(c => c.Status == Domain.Enums.CommissionStatus.Pending)
                    .Sum(c => c.Amount),
                TotalPaid = commissions
                    .Where(c => c.Status == Domain.Enums.CommissionStatus.Paid)
                    .Sum(c => c.Amount),
                TotalRecords = commissions.Count
            };
        }
    }
}
