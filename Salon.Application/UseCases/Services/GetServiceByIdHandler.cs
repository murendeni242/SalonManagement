using Salon.Application.DTOs.Services;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Services;

/// <summary>
/// Returns a single service by primary key, including soft-deleted ones
/// so admin users can inspect a deleted service by ID.
/// Same pattern as GetBookingByIdHandler.
/// </summary>
public class GetServiceByIdHandler
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceByIdHandler(IServiceRepository serviceRepository)
        => _serviceRepository = serviceRepository;

    /// <summary>
    /// Returns the service with the given <paramref name="id"/>, or null if not found.
    /// </summary>
    public async Task<ServiceDto?> Handle(int id)
    {
        var s = await _serviceRepository.GetByIdAsync(id);
        if (s is null) return null;

        return new ServiceDto
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
}