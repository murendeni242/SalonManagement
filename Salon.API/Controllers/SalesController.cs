using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.Sales;

namespace Salon.API.Controllers;

/// <summary>
/// Manages salon payment transactions.
///
/// Endpoints:
///   POST   /api/sales                      — record a payment (Owner, Reception)
///   POST   /api/sales/{id}/refund          — issue a refund (Owner)
///   POST   /api/sales/{id}/void            — void an erroneous entry (Owner)
///   GET    /api/sales                      — paginated list with revenue summary (Owner)
///   GET    /api/sales/{id}                 — single sale record (Owner, Reception)
///   GET    /api/sales/booking/{bookingId}  — all payments for a booking (Owner, Reception)
///   GET    /api/sales/{id}/audit           — change history (Owner)
/// </summary>
[ApiController]
[Route("api/sales")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly CreateSaleHandler _createHandler;
    private readonly RefundSaleHandler _refundHandler;
    private readonly VoidSaleHandler _voidHandler;
    private readonly GetSalesHandler _getAllHandler;
    private readonly GetSaleByIdHandler _getByIdHandler;
    private readonly GetSalesByBookingHandler _getByBookingHandler;
    private readonly GetSaleAuditLogsHandler _auditHandler;

    public SalesController(
        CreateSaleHandler createHandler,
        RefundSaleHandler refundHandler,
        VoidSaleHandler voidHandler,
        GetSalesHandler getAllHandler,
        GetSaleByIdHandler getByIdHandler,
        GetSalesByBookingHandler getByBookingHandler,
        GetSaleAuditLogsHandler auditHandler)
    {
        _createHandler = createHandler;
        _refundHandler = refundHandler;
        _voidHandler = voidHandler;
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
        _getByBookingHandler = getByBookingHandler;
        _auditHandler = auditHandler;
    }

    // POST /api/sales
    /// <summary>
    /// Records a new payment against a booking.
    /// A booking can have multiple payments (e.g. deposit + balance).
    /// Returns 201 Created with the sale record in the body.
    /// </summary>
    /// <response code="201">Payment recorded successfully.</response>
    /// <response code="400">Booking is Cancelled, invalid payment method, or amount is zero.</response>
    /// <response code="403">Caller does not have Owner or Reception role.</response>
    /// <response code="404">Booking ID does not exist.</response>
    [HttpPost]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> Create([FromBody] CreateSaleCommand command)
    {
        var sale = await _createHandler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
    }

    // POST /api/sales/{id}/refund
    /// <summary>
    /// Issues a refund against an existing Paid sale.
    /// Creates a new negative-amount sale record. The original is kept unchanged (status = Refunded).
    /// Partial refunds are supported — refundAmount can be less than the original AmountPaid.
    /// Owner role only.
    /// </summary>
    /// <param name="id">Primary key of the sale to refund.</param>
    /// <param name="command">Refund details.</param>
    /// <response code="200">Refund issued. Returns the new refund sale record.</response>
    /// <response code="400">Sale is not Paid, or refund amount exceeds original.</response>
    /// <response code="403">Caller does not have Owner role.</response>
    /// <response code="404">Sale ID does not exist.</response>
    [HttpPost("{id:int}/refund")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Refund(int id, [FromBody] RefundSaleCommand command)
    {
        command.SaleId = id;
        var refundSale = await _refundHandler.Handle(command);
        return Ok(refundSale);
    }

    // POST /api/sales/{id}/void
    /// <summary>
    /// Voids an erroneous Paid sale entry. NOT a refund — no money is returned.
    /// Use this to correct data-entry mistakes (wrong amount, wrong booking).
    /// Owner role only.
    /// </summary>
    /// <param name="id">Primary key of the sale to void.</param>
    /// <param name="command">Void reason.</param>
    /// <response code="204">Sale voided successfully.</response>
    /// <response code="400">Sale is not Paid, or reason is blank.</response>
    /// <response code="403">Caller does not have Owner role.</response>
    /// <response code="404">Sale ID does not exist.</response>
    [HttpPost("{id:int}/void")]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> Void(int id, [FromBody] VoidSaleCommand command)
    {
        command.SaleId = id;
        await _voidHandler.Handle(command);
        return NoContent();
    }

    // GET /api/sales
    /// <summary>
    /// Returns a paginated list of sales for an optional date range plus an aggregated
    /// revenue summary (total revenue, refunds, net, breakdown by payment method).
    /// Use this for the daily / weekly / monthly Owner revenue dashboard.
    /// Owner role only.
    /// </summary>
    /// <param name="from">Optional start date filter (inclusive), e.g. 2025-01-01.</param>
    /// <param name="to">Optional end date filter (inclusive), e.g. 2025-01-31.</param>
    /// <param name="skip">Records to skip. Defaults to 0.</param>
    /// <param name="take">Max records to return. Defaults to 50.</param>
    [HttpGet]
    [Authorize(Roles = Roles.Owner)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var result = await _getAllHandler.Handle(from, to, skip, take);
        return Ok(result);
    }

    // GET /api/sales/{id}
    /// <summary>
    /// Returns a single sale record by ID.
    /// </summary>
    /// <response code="404">Sale ID does not exist.</response>
    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sale = await _getByIdHandler.Handle(id);
        if (sale is null) return NotFound();
        return Ok(sale);
    }

    // GET /api/sales/booking/{bookingId}
    /// <summary>
    /// Returns the full payment history for a specific booking.
    /// Includes payments, refund records (negative amount), and voided entries.
    /// Useful on the booking detail screen to show total paid and any refunds.
    /// </summary>
    /// <param name="bookingId">Primary key of the booking.</param>
    [HttpGet("booking/{bookingId:int}")]
    [Authorize(Roles = $"{Roles.Owner},{Roles.Reception}")]
    public async Task<IActionResult> GetByBooking(int bookingId)
    {
        var sales = await _getByBookingHandler.Handle(bookingId);
        return Ok(sales);
    }

    // GET /api/sales/{id}/audit
    /// <summary>
    /// Returns the full change history for a sale record, oldest first.
    /// Each entry shows who made the change, what changed, and when.
    /// Uses the same shared audit log table as bookings and services.
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