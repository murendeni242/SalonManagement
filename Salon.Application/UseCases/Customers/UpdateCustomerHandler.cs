using Salon.Application.DTOs.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Customers;

/// <summary>
/// Updates a customer's personal details and writes an "Updated" audit entry
/// with before/after snapshot. Same pattern as UpdateStaffHandler.
/// </summary>
public class UpdateCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public UpdateCustomerHandler(
        ICustomerRepository customerRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _customerRepository = customerRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Workflow: load customer → snapshot old → apply changes → save → write audit → return DTO.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the customer ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when deleted, or new phone/email is already taken.</exception>
    public async Task<CustomerDto> Handle(UpdateCustomerCommand command)
    {
        var customer = await _customerRepository.GetByIdAsync(command.Id)
            ?? throw new NotFoundException("Customer", command.Id);

        var oldSnapshot = CreateCustomerHandler.Snapshot(customer);

        // Check phone uniqueness if it changed
        if (customer.Phone != command.Phone.Trim())
        {
            var existing = await _customerRepository.GetByPhoneAsync(command.Phone);
            if (existing != null && existing.Id != customer.Id)
                throw new DomainException($"Phone number '{command.Phone}' is already registered to another customer.");
        }

        // Check email uniqueness if it changed
        if (!string.IsNullOrWhiteSpace(command.Email) && customer.Email != command.Email.ToLower().Trim())
        {
            var existing = await _customerRepository.GetByEmailAsync(command.Email);
            if (existing != null && existing.Id != customer.Id)
                throw new DomainException($"Email '{command.Email}' is already registered to another customer.");
        }

        customer.UpdateDetails(
            command.FirstName, command.LastName,
            command.Phone, command.Email, command.DateOfBirth);

        await _customerRepository.UpdateAsync(customer);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Customer",
            entityId: customer.Id,
            action: "Updated",
            description: $"Customer '{customer.FullName}' updated by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            oldValues: oldSnapshot,
            newValues: CreateCustomerHandler.Snapshot(customer)));

        return CreateCustomerHandler.ToDto(customer);
    }
}