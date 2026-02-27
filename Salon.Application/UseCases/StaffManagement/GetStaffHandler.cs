using Salon.Application.DTOs.Staff;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.StaffManagement;

/// <summary>
/// Returns all non-deleted staff members.
/// Soft-deleted records are excluded automatically by the EF Core global query filter.
/// Same pattern as your existing GetStaffHandler — no breaking change.
/// </summary>
public class GetStaffHandler
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffHandler(IStaffRepository staffRepository)
        => _staffRepository = staffRepository;

    /// <summary>Returns all active (non-deleted) staff members.</summary>
    public async Task<IEnumerable<StaffDto>> Handle()
    {
        var staff = await _staffRepository.GetAllAsync();
        return staff.Select(CreateStaffHandler.ToDto);
    }
}