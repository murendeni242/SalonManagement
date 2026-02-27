using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.StaffManagement;

namespace Salon.API.Controllers;

/// <summary>
/// Manages salon staff profiles.
///
/// Endpoints:
///   POST   /api/staff                          — create profile (Owner)
///   PUT    /api/staff/{id}                     — update profile (Owner)
///   DELETE /api/staff/{id}                     — soft-delete profile (Owner)
///   GET    /api/staff                          — all non-deleted staff (Owner, Reception)
///   GET    /api/staff/{id}                     — single profile (Owner, Reception)
///   GET    /api/staff/{id}/schedule?date=      — daily schedule / calendar (Owner, Staff)
///   GET    /api/staff/{id}/audit               — change history (Owner)
/// </summary>
[ApiController]
[Route("api/staff")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly CreateStaffHandler _createHandler;
    private readonly UpdateStaffHandler _updateHandler;
    private readonly DeleteStaffHandler _deleteHandler;
    private readonly GetStaffHandler _getAllHandler;
    private readonly GetStaffByIdHandler _getByIdHandler;
    private readonly GetStaffScheduleHandler _scheduleHandler;
    private readonly GetStaffAuditLogsHandler _auditHandler;

    public StaffController(
        CreateStaffHandler createHandler,
        UpdateStaffHandler updateHandler,
        DeleteStaffHandler deleteHandler,
        GetStaffHandler getAllHandler,
        GetStaffByIdHandler getByIdHandler,
        GetStaffScheduleHandler scheduleHandler,
        GetStaffAuditLogsHandler auditHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _scheduleHandler = scheduleHandler;
        _auditHandler = auditHandler;
    }

    // POST /api/staff
    /// <summary>
    /// Creates a new staff profile. Owner role only.
    /// Returns 201 Created with a Location header and the new profile in the body.
    /// </summary>
    /// <response code="201">Staff profile created.</response>
    /// <response code="400">Validation failed (blank name, invalid role).</response>
    /// <response code="403">Caller does not have Owner role.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Create([FromBody] CreateStaffCommand command)
    {
        var staff = await _createHandler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
    }

    // PUT /api/staff/{id}
    /// <summary>
    /// Updates an existing staff profile including specialisations and status.
    /// Soft-deleted staff cannot be updated — returns 400.
    /// Owner role only.
    /// </summary>
    /// <param name="id">Primary key of the staff member to update.</param>
    /// <param name="command">Updated staff details.</param>
    /// <response code="200">Updated successfully.</response>
    /// <response code="400">Record is deleted or a required field is blank.</response>
    /// <response code="403">Caller does not have Owner role.</response>
    /// <response code="404">Staff ID does not exist.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStaffCommand command)
    {
        command.Id = id;
        var staff = await _updateHandler.Handle(command);
        return Ok(staff);
    }

    // DELETE /api/staff/{id}
    /// <summary>
    /// Soft-deletes a staff profile. The row stays in the database so historical
    /// bookings are never orphaned. Owner role only.
    /// </summary>
    /// <param name="id">Primary key of the staff member to delete.</param>
    /// <response code="204">Soft-deleted successfully.</response>
    /// <response code="400">Already deleted.</response>
    /// <response code="403">Caller does not have Owner role.</response>
    /// <response code="404">Staff ID does not exist.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Delete(int id)
    {
        await _deleteHandler.Handle(id);
        return NoContent();
    }

    // GET /api/staff
    /// <summary>
    /// Returns all non-deleted staff members ordered by name.
    /// Soft-deleted records are excluded automatically.
    /// Owner and Reception can see the full staff list.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _getAllHandler.Handle();
        return Ok(staff);
    }

    // GET /api/staff/{id}
    /// <summary>
    /// Returns a single staff profile by ID.
    /// Includes soft-deleted records so the Owner can inspect a deleted profile.
    /// </summary>
    /// <response code="404">Staff ID does not exist.</response>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> GetById(int id)
    {
        var staff = await _getByIdHandler.Handle(id);
        if (staff is null) return NotFound();
        return Ok(staff);
    }

    // GET /api/staff/{id}/schedule?date=2025-03-15
    /// <summary>
    /// Returns the daily schedule for a staff member on a specific date.
    /// Lists all non-cancelled bookings ordered by StartTime ascending.
    ///
    /// Used by:
    /// - Owner dashboard calendar to show each stylist's day.
    /// - Staff login to see their own appointments for the day.
    ///
    /// Defaults to today's date if no date is provided.
    /// </summary>
    /// <param name="id">Primary key of the staff member.</param>
    /// <param name="date">Date to retrieve the schedule for. Defaults to today.</param>
    /// <response code="404">Staff ID does not exist.</response>
    [HttpGet("{id:int}/schedule")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Staff}")]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;
        var schedule = await _scheduleHandler.Handle(id, targetDate);
        return Ok(schedule);
    }

    // GET /api/staff/{id}/audit
    /// <summary>
    /// Returns the full change history for a staff profile, oldest first.
    /// Uses the same shared audit log table as bookings, services, and sales.
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