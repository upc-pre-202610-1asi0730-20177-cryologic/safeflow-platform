using SafeFlow.API.EnvironmentalMonitoring.Application.Services;
using SafeFlow.API.EnvironmentalMonitoring.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.EnvironmentalMonitoring.Interfaces.REST;

[ApiController]
[Route("api/monitoring")]
public class EnvironmentalMonitoringController(IEnvironmentalMonitoringQueryService queryService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var dashboard = await queryService.GetDashboardAsync(ct);
        return Ok(new
        {
            kpis = dashboard.Kpis,
            monitorCards = dashboard.MonitorCards.Select(MonitoringCardAssembler.ToApiObject)
        });
    }
}
