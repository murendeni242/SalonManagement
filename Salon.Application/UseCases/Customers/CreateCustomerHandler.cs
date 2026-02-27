using Salon.Application.DTOs.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Customers;

/// <summary>
/// Creates a new customer record and writes a "Created" audit entry.
/// Guards against duplicate phone numbers and duplicate emails before saving.
/// Same pattern as all other Create handlers in this system.
/// </summary>
public class CreateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public CreateCustomerHandler(
        ICustomerRepository customerRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _customerRepository = customerRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Workflow: check for duplicate phone/email → create → save → write audit → return DTO.
    /// </summary>
    /// <param name="command">Validated input from the controller.</param>
    /// <returns>The newly created customer as a DTO.</returns>
    /// <exception cref="DomainException">Thrown when the phone or email is already registered.</exception>
    public async Task<CustomerDto> Handle(CreateCustomerCommand command)
    {
        // Check for duplicate phone — the primary lookup key
        var existingByPhone = await _customerRepository.GetByPhoneAsync(command.Phone);
        if (existingByPhone != null)
            throw new DomainException($"A customer with phone number '{command.Phone}' already exists.");

        // Check for duplicate email when provided
        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            var existingByEmail = await _customerRepository.GetByEmailAsync(command.Email);
            if (existingByEmail != null)
                throw new DomainException($"A customer with email '{command.Email}' already exists.");
        }

        var customer = new Customer(
            command.FirstName,
            command.LastName,
            command.Phone,
            command.Email,
            command.DateOfBirth);

        if (!string.IsNullOrWhiteSpace(command.Notes))
            customer.UpdateNotes(command.Notes);

        await _customerRepository.AddAsync(customer);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Customer",
            entityId: customer.Id,
            action: "Created",
            description: $"Customer '{customer.FullName}' created by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            newValues: Snapshot(customer)));

        return ToDto(customer);
    }

    internal static string Snapshot(Customer c) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            c.FirstName,
            c.LastName,
            c.Phone,
            c.Email,
            c.DateOfBirth,
            c.Notes
        });

    internal static CustomerDto ToDto(Customer c) => new()
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        FullName = c.FullName,
        Phone = c.Phone,
        Email = c.Email,
        DateOfBirth = c.DateOfBirth,
        Notes = c.Notes,
        LastVisitAt = c.LastVisitAt,
        IsDeleted = c.IsDeleted,
        DeletedAt = c.DeletedAt
    };
}