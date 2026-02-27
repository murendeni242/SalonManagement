using Salon.Application.DTOs.Customers;
using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Customers;

/// <summary>
/// Updates ONLY the notes field on a customer record — allergies, colour formulas, preferences.
/// Kept separate so reception can add notes without accidentally overwriting personal details.
/// Writes an "NotesUpdated" audit entry.
/// </summary>
public class UpdateCustomerNotesHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditLogRepository _auditLog;
    private readonly ICurrentUserService _currentUser;

    public UpdateCustomerNotesHandler(
        ICustomerRepository customerRepository,
        IAuditLogRepository auditLog,
        ICurrentUserService currentUser)
    {
        _customerRepository = customerRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Updates only the notes field for the customer with the given ID.
    /// </summary>
    /// <exception cref="NotFoundException">Thrown when the customer ID does not exist.</exception>
    /// <exception cref="DomainException">Thrown when deleted or notes exceed 2000 characters.</exception>
    public async Task<CustomerDto> Handle(UpdateCustomerNotesCommand command)
    {
        var customer = await _customerRepository.GetByIdAsync(command.Id)
            ?? throw new NotFoundException("Customer", command.Id);

        var oldNotes = customer.Notes;

        customer.UpdateNotes(command.Notes);
        await _customerRepository.UpdateAsync(customer);

        await _auditLog.AddAsync(new AuditLog(
            entityName: "Customer",
            entityId: customer.Id,
            action: "NotesUpdated",
            description: $"Notes for customer '{customer.FullName}' updated by {_currentUser.UserEmail}.",
            changedBy: _currentUser.UserEmail,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { Notes = oldNotes }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new { Notes = command.Notes })));

        return CreateCustomerHandler.ToDto(customer);
    }
}
