using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.Customers;

namespace Salon.API.Controllers;

/// <summary>
/// Manages salon customer records.
///
/// Endpoints:
///   POST   /api/customers                     — create record (Owner, Reception)
///   PUT    /api/customers/{id}                — update details (Owner, Reception)
///   PATCH  /api/customers/{id}/notes          — update notes only (Owner, Reception)
///   DELETE /api/customers/{id}                — soft-delete (Owner)
///   GET    /api/customers                     — paginated list (Owner, Reception)
///   GET    /api/customers/{id}                — single record (Owner, Reception)
///   GET    /api/customers/search?q=           — search by name or phone (Owner, Reception)
///   GET    /api/customers/{id}/profile        — full profile + history + spend (Owner, Reception)
///   GET    /api/customers/{id}/audit          — change history (Owner)
/// </summary>
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly CreateCustomerHandler _createHandler;
    private readonly UpdateCustomerHandler _updateHandler;
    private readonly UpdateCustomerNotesHandler _notesHandler;
    private readonly DeleteCustomerHandler _deleteHandler;
    private readonly GetAllCustomersHandler _getAllHandler;
    private readonly GetCustomerByIdHandler _getByIdHandler;
    private readonly SearchCustomersHandler _searchHandler;
    private readonly GetCustomerProfileHandler _profileHandler;
    private readonly GetCustomerAuditLogsHandler _auditHandler;

    public CustomersController(
        CreateCustomerHandler createHandler,
        UpdateCustomerHandler updateHandler,
        UpdateCustomerNotesHandler notesHandler,
        DeleteCustomerHandler deleteHandler,
        GetAllCustomersHandler getAllHandler,
        GetCustomerByIdHandler getByIdHandler,
        SearchCustomersHandler searchHandler,
        GetCustomerProfileHandler profileHandler,
        GetCustomerAuditLogsHandler auditHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _notesHandler = notesHandler;
        _deleteHandler = deleteHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _searchHandler = searchHandler;
        _profileHandler = profileHandler;
        _auditHandler = auditHandler;
    }

    // POST /api/customers
    /// <summary>
    /// Creates a new customer. Guards against duplicate phone numbers and emails.
    /// Returns 201 Created with the customer record in the body.
    /// </summary>
    /// <response code="201">Customer created.</response>
    /// <response code="400">Duplicate phone/email or required field missing.</response>
    /// <response code="403">Caller does not have Owner or Reception role.</response>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var customer = await _createHandler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    // PUT /api/customers/{id}
    /// <summary>
    /// Updates a customer's personal details (name, phone, email, date of birth).
    /// Does NOT update notes — use PATCH /notes for that.
    /// </summary>
    /// <response code="200">Updated successfully.</response>
    /// <response code="400">Record is deleted, required field is blank, or phone/email already taken.</response>
    /// <response code="404">Customer ID does not exist.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerCommand command)
    {
        command.Id = id;
        var customer = await _updateHandler.Handle(command);
        return Ok(customer);
    }

    // PATCH /api/customers/{id}/notes
    /// <summary>
    /// Updates ONLY the notes field on a customer — allergies, colour formulas, preferences.
    /// Kept separate so staff can update notes without touching personal details.
    /// </summary>
    /// <response code="200">Notes updated successfully.</response>
    /// <response code="400">Notes exceed 2000 characters or record is deleted.</response>
    /// <response code="404">Customer ID does not exist.</response>
    [HttpPatch("{id:int}/notes")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdateCustomerNotesCommand command)
    {
        command.Id = id;
        var customer = await _notesHandler.Handle(command);
        return Ok(customer);
    }

    // DELETE /api/customers/{id}
    /// <summary>
    /// Soft-deletes a customer. Row kept in database — bookings and sales are never orphaned.
    /// Owner role only.
    /// </summary>
    /// <response code="204">Soft-deleted successfully.</response>
    /// <response code="400">Already deleted.</response>
    /// <response code="403">Caller does not have Owner role.</response>
    /// <response code="404">Customer ID does not exist.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Delete(int id)
    {
        await _deleteHandler.Handle(id);
        return NoContent();
    }

    // GET /api/customers
    /// <summary>
    /// Returns a paginated alphabetical list of non-deleted customers.
    /// </summary>
    /// <param name="skip">Records to skip. Defaults to 0.</param>
    /// <param name="take">Max records to return. Defaults to 50.</param>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var customers = await _getAllHandler.Handle(skip, take);
        return Ok(customers);
    }

    // GET /api/customers/{id}
    /// <summary>
    /// Returns a single customer record by ID including soft-deleted records.
    /// </summary>
    /// <response code="404">Customer ID does not exist.</response>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _getByIdHandler.Handle(id);
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    // GET /api/customers/search?q=jane
    /// <summary>
    /// Searches customers by partial name or phone number.
    /// Used by reception to find a customer quickly when they arrive or call.
    /// Returns maximum 20 results. Requires at least 2 characters.
    /// </summary>
    /// <param name="q">Search term: partial first name, last name, full name, or phone number.</param>
    [HttpGet("search")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return BadRequest("Search term must be at least 2 characters.");

        var results = await _searchHandler.Handle(q);
        return Ok(results);
    }

    // GET /api/customers/{id}/profile
    /// <summary>
    /// Returns the full customer profile for the detail screen:
    /// personal details + last 5 bookings + total visits + total spend + days since last visit.
    /// This is the "customer card" — everything staff need before an appointment.
    /// </summary>
    /// <response code="404">Customer ID does not exist.</response>
    [HttpGet("{id:int}/profile")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> GetProfile(int id)
    {
        var profile = await _profileHandler.Handle(id);
        return Ok(profile);
    }

    // GET /api/customers/{id}/audit
    /// <summary>
    /// Returns the full change history for a customer record, oldest first.
    /// Uses the same shared audit log table as all other entities. Owner only.
    /// </summary>
    [HttpGet("{id:int}/audit")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> GetAuditLog(int id)
    {
        var logs = await _auditHandler.Handle(id);
        return Ok(logs);
    }
}