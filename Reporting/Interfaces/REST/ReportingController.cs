using SafeFlow.API.Reporting.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Reporting.Interfaces.REST;

[ApiController]
[Route("api/reporting")]
public class ReportingController(IReportingQueryService queryService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await queryService.GetDashboardAsync(ct));
}
