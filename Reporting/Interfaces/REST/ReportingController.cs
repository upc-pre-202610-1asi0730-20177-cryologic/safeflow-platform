using SafeFlow.API.Reporting.Application.Services;
using SafeFlow.API.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Reporting.Interfaces.REST;

[ApiController]
[Route("api/reporting")]
[Authorize]
public class ReportingController(IReportingQueryService queryService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await queryService.GetDashboardAsync(ct));
}
