using SafeFlow.API.Analytics.Application.Services;
using SafeFlow.API.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Analytics.Interfaces.REST;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController(IAnalyticsQueryService queryService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await queryService.GetDashboardAsync(ct));
}
