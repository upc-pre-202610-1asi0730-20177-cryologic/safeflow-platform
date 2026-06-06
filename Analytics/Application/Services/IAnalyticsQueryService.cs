namespace SafeFlow.API.Analytics.Application.Services;

public interface IAnalyticsQueryService
{
    Task<object> GetDashboardAsync(CancellationToken ct = default);
}
