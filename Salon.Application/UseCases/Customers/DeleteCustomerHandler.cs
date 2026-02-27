using Salon.Application.DTOs.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Customers;

/// <summary>
/// Soft-deletes a customer record. Row kept in database — bookings and sales are never orphaned.
/// Same pattern as DeleteStaffHandler and DeleteServiceHandler.
/// Owner role only.
/// </summary>
public class DeleteCustomerHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public DeleteCustomerHandler(
        ICustomerRepository customerRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _customerRepository = customerRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Soft-deletes the customer with the given <paramref name="customerId"/>.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the customer ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when already deleted.</exception>
    public async Task Handle(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId)
            ?? throw new NotFoundException("Customer", customerId);

        customer.SoftDelete();
        await _customerRepository.UpdateAsync(customer);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Customer",
            entityId: customer.Id,
            action: "SoftDeleted",
            description: $"Customer '{customer.FullName}' deleted by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail));
    }
}