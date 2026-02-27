using Salon.Application.DTOs.Staff;
using Salon.Application.UseCases.StaffManagement;

using Salon.Domain.Interfaces;

/// <summary>
/// Returns a single staff member by primary key.
/// Includes soft-deleted records so the Owner can inspect a deleted profile by ID.
/// Same pattern as your existing GetStaffByIdHandler.
/// </summary>
public class GetStaffByIdHandler
{
    private readonly IStaffRepository _staffRepository;

    public GetStaffByIdHandler(IStaffRepository staffRepository)
        => _staffRepository = staffRepository;

    /// <summary>
    /// Returns the staff member with the given <paramref name="id"/>, or null if not found.
    /// </summary>
    public async Task<StaffDto?> Handle(int id)
    {
        var s = await _staffRepository.GetByIdAsync(id);
        return s is null ? null : CreateStaffHandler.ToDto(s);
    }
}