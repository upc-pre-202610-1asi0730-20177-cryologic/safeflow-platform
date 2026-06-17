namespace SafeFlow.API.Reporting.Application.Services;

public interface IReportingQueryService
{
    Task<object> GetDashboardAsync(CancellationToken ct = default);
}
