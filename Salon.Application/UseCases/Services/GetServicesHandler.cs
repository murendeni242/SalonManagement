using Salon.Application.DTOs.Services;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Services;

/// <summary>
/// Returns all non-deleted services.
/// Soft-deleted services are excluded automatically by the EF Core global query filter.
/// Same pattern as your original — no changes needed here.
/// </summary>
public class GetServicesHandler
{
    private readonly IServiceRepository _serviceRepository;

    public GetServicesHandler(IServiceRepository serviceRepository)
        => _serviceRepository = serviceRepository;

    /// <summary>Returns all active (non-deleted) services.</summary>
    public async Task<IEnumerable<ServiceDto>> Handle()
    {
        var services = await _serviceRepository.GetAllAsync();
        return services.Select(ToDto);
    }

    private static ServiceDto ToDto(Domain.Entities.Service s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        DurationMinutes = s.DurationMinutes,
        BasePrice = s.BasePrice,
        Status = s.Status,
        IsDeleted = s.IsDeleted,
        DeletedAt = s.DeletedAt
    };
}