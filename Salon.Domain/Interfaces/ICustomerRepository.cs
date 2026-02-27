using Salon.Domain.Entities;

namespace Salon.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Customer entity.
/// Your existing AddAsync and GetAllAsync are kept — five new methods added.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>Saves a new customer to the database.</summary>
    Task AddAsync(Customer customer);

    /// <summary>
    /// Returns a customer by primary key including soft-deleted records, or null if not found.
    /// Uses IgnoreQueryFilters() so the handler can still load a deleted record.
    /// </summary>
    Task<Customer?> GetByIdAsync(int id);

    /// <summary>
    /// Returns a page of non-deleted customers ordered by LastName, FirstName.
    /// Pagination runs in SQL — no full table load.
    /// </summary>
    /// <param name="skip">Records to skip.</param>
    /// <param name="take">Maximum records to return.</param>
    Task<IEnumerable<Customer>> GetPagedAsync(int skip, int take);

    /// <summary>
    /// Searches non-deleted customers by name or phone number.
    /// Used by reception to quickly find a customer when they arrive or call.
    /// Matches on: FirstName, LastName, FullName (first + last), or Phone (starts-with).
    /// Returns a maximum of 20 results to keep the picker fast.
    /// </summary>
    /// <param name="searchTerm">Partial name or phone number to search for.</param>
    Task<IEnumerable<Customer>> SearchAsync(string searchTerm);   // ✅ NEW

    /// <summary>
    /// Returns a customer by their email address, or null if not found.
    /// Used to prevent duplicate registrations.
    /// </summary>
    Task<Customer?> GetByEmailAsync(string email);                // ✅ NEW

    /// <summary>
    /// Returns a customer by their phone number, or null if not found.
    /// Used to check for duplicates before creating a new customer.
    /// </summary>
    Task<Customer?> GetByPhoneAsync(string phone);                // ✅ NEW

    /// <summary>Saves changes to an existing customer record.</summary>
    Task UpdateAsync(Customer customer);                          // ✅ NEW

    Task<IEnumerable<Customer>> GetAllAsync();
}