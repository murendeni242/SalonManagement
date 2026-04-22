using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.Commissions;

namespace Salon.API.Controllers;

/// <summary>
/// Commission management endpoints.
/// </summary>
[ApiController]
[Route("api/commissions")]
[Authorize(Roles = Roles.Owner)]
public class CommissionController : ControllerBase
{
    private readonly GetCommissionRulesHandler _getRulesHandler;
    private readonly UpsertCommissionRuleHandler _upsertRuleHandler;
    private readonly GetStaffCommissionsHandler _getStaffHandler;
    private readonly MarkCommissionPaidHandler _markPaidHandler;

    public CommissionController(
        GetCommissionRulesHandler getRulesHandler,
        UpsertCommissionRuleHandler upsertRuleHandler,
        GetStaffCommissionsHandler getStaffHandler,
        MarkCommissionPaidHandler markPaidHandler)
    {
        _getRulesHandler = getRulesHandler;
        _upsertRuleHandler = upsertRuleHandler;
        _getStaffHandler = getStaffHandler;
        _markPaidHandler = markPaidHandler;
    }

    /// <summary>Returns all commission rules with their tiers.</summary>
    [HttpGet("rules")]
    public async Task<IActionResult> GetRules()
    {
        var rules = await _getRulesHandler.Handle();
        return Ok(rules);
    }

    // PUT /api/commissions/rules/{staffId}
    /// <summary>
    /// Creates or updates the commission rule for a staff member.
    /// For tiered rules include a Tiers array — previous tiers are replaced.
    /// </summary>
    [HttpPut("rules/{staffId:int}")]
    public async Task<IActionResult> UpsertRule(int staffId, [FromBody] UpsertCommissionRuleCommand command)
    {
        command.StaffId = staffId;
        var result = await _upsertRuleHandler.Handle(command);
        return Ok(result);
    }

    /// <summary>
    /// Returns commission summary for a staff member within a date range.
    /// Defaults to current calendar month if no dates provided.
    /// </summary>
    [HttpGet("staff/{staffId:int}")]
    public async Task<IActionResult> GetStaffCommissions(int staffId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var fromDate = from ?? monthStart;
        var toDate = to ?? now;

        var result = await _getStaffHandler.Handle(staffId, fromDate, toDate);
        return Ok(result);
    }

    /// <summary>
    /// Marks commissions as paid for a staff member.
    /// Pass a list of CommissionIds to mark specific ones, or an empty list to mark ALL pending commissions for this staff member as paid.
    /// </summary>
    [HttpPost("pay/{staffId:int}")]
    public async Task<IActionResult> MarkPaid(int staffId, [FromBody] MarkCommissionPaidCommand command)
    {
        command.StaffId = staffId;
        var result = await _markPaidHandler.Handle(command);
        return Ok(result);
    }
}