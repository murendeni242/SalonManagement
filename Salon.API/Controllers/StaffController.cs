using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.StaffManagement;
using Salon.Application.UseCases.StaffSchedules;

namespace Salon.API.Controllers;

/// <summary>
/// Manages salon staff profiles and working hours configuration.
///
/// Endpoints:
///   POST   /api/staff                                  — create profile (Owner)
///   PUT    /api/staff/{id}                             — update profile (Owner)
///   DELETE /api/staff/{id}                             — soft-delete profile (Owner)
///   GET    /api/staff                                  — all non-deleted staff (Owner, Reception)
///   GET    /api/staff/{id}                             — single profile (Owner, Reception)
///   GET    /api/staff/{id}/schedule?date=              — daily schedule / calendar (Owner, Staff)
///   GET    /api/staff/{id}/audit                       — change history (Owner)
///   GET    /api/staff/{id}/working-hours               — weekly working hours (Owner, Reception)
///   PUT    /api/staff/{id}/working-hours               — upsert one day's hours (Owner)
///   DELETE /api/staff/{id}/working-hours/{day}         — remove one day (Owner)
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
    private readonly GetWeeklyScheduleHandler _getWorkingHoursHandler;
    private readonly UpsertStaffScheduleHandler _upsertWorkingHoursHandler;
    private readonly DeleteStaffScheduleHandler _deleteWorkingHoursHandler;

    public StaffController(
        CreateStaffHandler createHandler,
        UpdateStaffHandler updateHandler,
        DeleteStaffHandler deleteHandler,
        GetStaffHandler getAllHandler,
        GetStaffByIdHandler getByIdHandler,
        GetStaffScheduleHandler scheduleHandler,
        GetStaffAuditLogsHandler auditHandler,
        GetWeeklyScheduleHandler getWorkingHoursHandler,
        UpsertStaffScheduleHandler upsertWorkingHoursHandler,
        DeleteStaffScheduleHandler deleteWorkingHoursHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _scheduleHandler = scheduleHandler;
        _auditHandler = auditHandler;
        _getWorkingHoursHandler = getWorkingHoursHandler;
        _upsertWorkingHoursHandler = upsertWorkingHoursHandler;
        _deleteWorkingHoursHandler = deleteWorkingHoursHandler;
    }

    // ── Staff profile endpoints ───────────────────────────────────────────────

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
    /// Returns the daily appointment schedule for a staff member on a specific date.
    /// Lists all non-cancelled bookings ordered by StartTime ascending.
    /// Defaults to today's date if no date is provided.
    /// </summary>
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
    /// Owner role only.
    /// </summary>
    [HttpGet("{id:int}/audit")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> GetAuditLog(int id)
    {
        var logs = await _auditHandler.Handle(id);
        return Ok(logs);
    }

    // ── Working hours endpoints ───────────────────────────────────────────────

    // GET /api/staff/{id}/working-hours
    /// <summary>
    /// Returns the full weekly working hours configuration for a staff member.
    /// One entry per configured day — days with no entry mean the staff member
    /// does not work that day and cannot be booked.
    /// Owner and Reception can view working hours.
    /// </summary>
    /// <param name="id">Primary key of the staff member.</param>
    /// <response code="200">Weekly working hours returned.</response>
    /// <response code="404">Staff ID does not exist.</response>
    [HttpGet("{id:int}/working-hours")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> GetWorkingHours(int id)
    {
        var schedule = await _getWorkingHoursHandler.Handle(id);
        return Ok(schedule);
    }

    // PUT /api/staff/{id}/working-hours
    /// <summary>
    /// Creates or updates working hours for a staff member on one day of the week.
    /// If a row already exists for that day it is updated — otherwise a new row is created.
    /// Owner role only.
    ///
    /// Example body:
    /// {
    ///   "dayOfWeek": 1,
    ///   "startTime": "09:00:00",
    ///   "endTime": "17:00:00"
    /// }
    /// </summary>
    /// <param name="id">Primary key of the staff member.</param>
    /// <param name="command">Day of week and working hours to set.</param>
    /// <response code="200">Working hours saved.</response>
    /// <response code="400">End time is not after start time.</response>
    /// <response code="404">Staff ID does not exist.</response>
    [HttpPut("{id:int}/working-hours")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> UpsertWorkingHours(
        int id,
        [FromBody] UpsertStaffScheduleCommand command)
    {
        command.StaffId = id;
        var result = await _upsertWorkingHoursHandler.Handle(command);
        return Ok(result);
    }

    // DELETE /api/staff/{id}/working-hours/{day}
    /// <summary>
    /// Removes working hours for a staff member on a specific day of the week.
    /// After deletion the staff member cannot be booked on that day.
    /// Hard delete — configuration data, no audit trail needed.
    /// Owner role only.
    /// </summary>
    /// <param name="id">Primary key of the staff member.</param>
    /// <param name="day">
    /// Day of week to remove (0=Sunday, 1=Monday ... 6=Saturday).
    /// </param>
    /// <response code="204">Working hours removed.</response>
    /// <response code="404">No working hours found for this staff member on this day.</response>
    [HttpDelete("{id:int}/working-hours/{day:int}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> DeleteWorkingHours(int id, int day)
    {
        var command = new DeleteStaffScheduleCommand
        {
            StaffId = id,
            DayOfWeek = (DayOfWeek)day
        };
        await _deleteWorkingHoursHandler.Handle(command);
        return NoContent();
    }
}