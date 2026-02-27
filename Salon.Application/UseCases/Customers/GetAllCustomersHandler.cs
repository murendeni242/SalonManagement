using Salon.Application.DTOs;
using Salon.Application.DTOs.Customers;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Customers;

/// <summary>
/// Returns a paginated list of non-deleted customers ordered by last name.
/// Soft-deleted customers excluded automatically by the EF Core global query filter.
/// </summary>
public class GetAllCustomersHandler
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomersHandler(ICustomerRepository customerRepository)
        => _customerRepository = customerRepository;

    /// <param name="skip">Records to skip. Defaults to 0.</param>
    /// <param name="take">Max records to return. Defaults to 50.</param>
    public async Task<IEnumerable<CustomerDto>> Handle(int skip = 0, int take = 50)
    {
        var customers = await _customerRepository.GetPagedAsync(skip, take);
        return customers.Select(CreateCustomerHandler.ToDto);
    }
}
