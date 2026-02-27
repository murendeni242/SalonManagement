using Salon.Application.DTOs.Customers;
using Salon.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon.Application.UseCases.Customers
{
    /// <summary>
    /// Searches for customers by partial name or phone number.
    /// Used by reception to quickly find a customer when they arrive or call in.
    /// Returns a maximum of 20 results — enough for a fast picker, not a full list.
    /// </summary>
    public class SearchCustomersHandler
    {
        private readonly ICustomerRepository _customerRepository;

        public SearchCustomersHandler(ICustomerRepository customerRepository)
            => _customerRepository = customerRepository;

        /// <param name="searchTerm">Partial name (first, last, or full) or phone number.</param>
        public async Task<IEnumerable<CustomerDto>> Handle(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<CustomerDto>();

            var results = await _customerRepository.SearchAsync(searchTerm.Trim());
            return results.Select(CreateCustomerHandler.ToDto);
        }
    }
}
