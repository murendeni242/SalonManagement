using Salon.Application.DTOs;
using Salon.Application.DTOs.Customers;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Customers;

/// <summary>
/// Returns a single customer by primary key including soft-deleted records.
/// </summary>
public class GetCustomerByIdHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdHandler(ICustomerRepository customerRepository)
        => _customerRepository = customerRepository;

    /// <summary>Returns the customer with the given <paramref name="id"/>, or null if not found.</summary>
    public async Task<CustomerDto?> Handle(int id)
    {
        var c = await _customerRepository.GetByIdAsync(id);
        return c is null ? null : CreateCustomerHandler.ToDto(c);
    }
}