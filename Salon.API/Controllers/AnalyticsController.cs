using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salon.Application.Auth;
using Salon.Application.UseCases.Analytics;

namespace Salon.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = Roles.Owner)]
public class AnalyticsController : ControllerBase
{
    private readonly GetDashboardAnalyticsHandler _handler;

    public AnalyticsController(GetDashboardAnalyticsHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Returns all chart data for the dashboard.
    /// Defaults to the last 30 days if no dates provided.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var toDate = to ?? DateTime.UtcNow;
        var fromDate = from ?? toDate.AddDays(-30);

        var result = await _handler.Handle(fromDate, toDate);
        return Ok(result);
    }
}