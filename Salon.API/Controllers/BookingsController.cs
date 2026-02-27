using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.Bookings;

namespace Salon.API.Controllers;

/// <summary>
/// Manages salon bookings.
/// All endpoints require a valid JWT — unauthenticated requests get 401.
/// Role restrictions are documented on each action.
/// </summary>
[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly CreateBookingHandler _createHandler;
    private readonly UpdateBookingHandler _updateHandler;
    private readonly ConfirmBookingHandler _confirmHandler;
    private readonly CancelBookingHandler _cancelHandler;
    private readonly DeleteBookingHandler _deleteHandler;
    private readonly GetBookingsHandler _getAllHandler;
    private readonly GetBookingByIdHandler _getByIdHandler;
    private readonly GetAuditLogsHandler _auditHandler;
    private readonly CompleteBookingHandler _completeHandler;

    public BookingsController(
        CreateBookingHandler createHandler,
        UpdateBookingHandler updateHandler,
        ConfirmBookingHandler confirmHandler,
        CancelBookingHandler cancelHandler,
        DeleteBookingHandler deleteHandler,
        GetBookingsHandler getAllHandler,
        GetBookingByIdHandler getByIdHandler,
        GetAuditLogsHandler auditHandler,
        CompleteBookingHandler completeHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _confirmHandler = confirmHandler;
        _cancelHandler = cancelHandler;
        _deleteHandler = deleteHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _auditHandler = auditHandler;
        _completeHandler = completeHandler;
    }

    // POST api/bookings
    /// <summary>
    /// Creates a new booking. End time is calculated from the service duration.
    /// Returns 201 with the booking in the body and a Location header.
    /// </summary>
    /// <response code="201">Booking created successfully.</response>
    /// <response code="400">Slot taken, date in the past, or notes too long.</response>
    /// <response code="403">Caller does not have Owner or Reception role.</response>
    /// <response code="404">Service ID does not exist.</response>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Create([FromBody] CreateBookingCommand command)
    {
        var booking = await _createHandler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    // PUT api/bookings/{id}
    /// <summary>
    /// Updates scheduling fields on a Pending booking.
    /// Confirmed, Completed and Cancelled bookings are immutable.
    /// </summary>
    /// <response code="200">Booking updated successfully.</response>
    /// <response code="400">Not Pending, slot conflict, or date in the past.</response>
    /// <response code="403">Caller does not have Owner or Reception role.</response>
    /// <response code="404">Booking or Service ID does not exist.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingCommand command)
    {
        command.Id = id;
        var booking = await _updateHandler.Handle(command);
        return Ok(booking);
    }

    // POST api/bookings/{id}/confirm
    /// <summary>
    /// Confirms a Pending booking, transitioning it to Confirmed state.
    /// </summary>
    /// <response code="204">Confirmed successfully.</response>
    /// <response code="400">Booking is not Pending.</response>
    /// <response code="403">Caller does not have Owner or Reception role.</response>
    /// <response code="404">Booking ID does not exist.</response>
    [HttpPost("{id:int}/confirm")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Confirm(int id)
    {
        await _confirmHandler.Handle(id);
        return NoContent();
    }

    // POST api/bookings/{id}/cancel
    /// <summary>
    /// Cancels a Pending or Confirmed booking.
    /// Completed bookings cannot be cancelled.
    /// </summary>
    /// <response code="204">Cancelled successfully.</response>
    /// <response code="400">Already Completed or Cancelled.</response>
    /// <response code="403">Caller does not have Owner or Reception role.</response>
    /// <response code="404">Booking ID does not exist.</response>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _cancelHandler.Handle(id);
        return NoContent();
    }

    // DELETE api/bookings/{id}
    /// <summary>
    /// Soft-deletes a booking. Row stays in the database for the audit trail
    /// but is hidden from all normal queries. Owner only.
    /// </summary>
    /// <response code="204">Soft-deleted successfully.</response>
    /// <response code="400">Already deleted.</response>
    /// <response code="403">Caller does not have Owner role.</response>
    /// <response code="404">Booking ID does not exist.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Delete(int id)
    {
        await _deleteHandler.Handle(id);
        return NoContent();
    }

    // GET api/bookings
    /// <summary>
    /// Paginated list of non-deleted bookings ordered by BookingDate descending.
    /// Pagination runs in SQL.
    /// </summary>
    /// <param name="skip">Records to skip. Defaults to 0.</param>
    /// <param name="take">Max records to return. Defaults to 50.</param>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var bookings = await _getAllHandler.Handle(skip, take);
        return Ok(bookings);
    }

    // GET api/bookings/{id}
    /// <summary>
    /// Returns a single booking by ID. Includes soft-deleted bookings
    /// so admins can inspect them.
    /// </summary>
    /// <response code="404">Booking ID does not exist.</response>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _getByIdHandler.Handle(id);
        if (booking is null) return NotFound();
        return Ok(booking);
    }

    // GET api/bookings/{id}/audit
    /// <summary>
    /// Returns the full change history for this booking, oldest first.
    /// Each entry shows who made the change, what changed, and when.
    /// Owner only.
    /// </summary>
    [HttpGet("{id:int}/audit")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> GetAuditLog(int id)
    {
        var logs = await _auditHandler.Handle(id);
        return Ok(logs);
    }

    // POST api/bookings/{id}/complete
    /// <summary>
    /// Marks a confirmed booking as completed.
    /// Only bookings in <c>Confirmed</c> status can be completed.
    /// </summary>
    /// <param name="id">The booking ID to complete.</param>
    /// <response code="200">Booking marked as completed successfully.</response>
    /// <response code="400">Booking is not in Confirmed status.</response>
    /// <response code="404">Booking ID does not exist.</response>
    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Complete(int id)
    {
        await _completeHandler.Handle(id);
        return Ok();
    }
}