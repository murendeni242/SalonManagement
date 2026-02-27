using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.Services;

namespace Salon.API.Controllers;

/// <summary>
/// Manages salon services (e.g. Haircut, Colour, Treatment).
/// All endpoints require a valid JWT. Role restrictions documented per action.
/// Built directly on top of your original ServicesController — same structure,
/// three new endpoints added: PUT update, DELETE soft-delete, GET audit history.
/// </summary>
[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly CreateServiceHandler _createHandler;
    private readonly UpdateServiceHandler _updateHandler;
    private readonly DeleteServiceHandler _deleteHandler;
    private readonly GetServicesHandler _getAllHandler;
    private readonly GetServiceByIdHandler _getByIdHandler;
    private readonly GetServiceAuditLogsHandler _auditHandler;

    public ServicesController(
        CreateServiceHandler createHandler,
        UpdateServiceHandler updateHandler,
        DeleteServiceHandler deleteHandler,
        GetServicesHandler getAllHandler,
        GetServiceByIdHandler getByIdHandler,
        GetServiceAuditLogsHandler auditHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _auditHandler = auditHandler;
    }

    // POST /api/services
    /// <summary>
    /// Creates a new service. Owner role only.
    /// Returns 201 Created with a Location header pointing to the new resource.
    /// </summary>
    /// <response code="201">Service created successfully.</response>
    /// <response code="400">Validation failed (blank name, zero duration, negative price).</response>
    /// <response code="403">Caller does not have Owner role.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Create([FromBody] CreateServiceCommand command)
    {
        var id = await _createHandler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    // PUT /api/services/{id}
    /// <summary>
    /// Updates an existing service. Owner role only.
    /// Soft-deleted services cannot be updated — returns 400.
    /// </summary>
    /// <param name="id">Primary key of the service to update.</param>
    /// <param name="command">Updated service details.</param>
    /// <response code="200">Service updated successfully.</response>
    /// <response code="400">Service is deleted or a validation rule is violated.</response>
    /// <response code="403">Caller does not have Owner role.</response>
    /// <response code="404">Service ID does not exist.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceCommand command)
    {
        command.Id = id;
        var service = await _updateHandler.Handle(command);
        return Ok(service);
    }

    // DELETE /api/services/{id}
    /// <summary>
    /// Soft-deletes a service. The row stays in the database so historical
    /// bookings and the audit trail are never broken. Owner role only.
    /// </summary>
    /// <param name="id">Primary key of the service to delete.</param>
    /// <response code="204">Service soft-deleted successfully.</response>
    /// <response code="400">Service is already deleted.</response>
    /// <response code="403">Caller does not have Owner role.</response>
    /// <response code="404">Service ID does not exist.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Delete(int id)
    {
        await _deleteHandler.Handle(id);
        return NoContent();
    }

    // GET /api/services
    /// <summary>
    /// Returns all non-deleted services.
    /// Soft-deleted services are excluded automatically.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _getAllHandler.Handle();
        return Ok(services);
    }

    // GET /api/services/{id}
    /// <summary>
    /// Returns a single service by ID. Includes soft-deleted services
    /// so admins can inspect them.
    /// </summary>
    /// <response code="404">Service ID does not exist.</response>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _getByIdHandler.Handle(id);
        if (service is null) return NotFound();
        return Ok(service);
    }

    // GET /api/services/{id}/audit
    /// <summary>
    /// Returns the full change history for this service, oldest first.
    /// Uses the same shared audit log table as bookings — entityName = "Service".
    /// Owner role only.
    /// </summary>
    [HttpGet("{id:int}/audit")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> GetAuditLog(int id)
    {
        var logs = await _auditHandler.Handle(id);
        return Ok(logs);
    }
}