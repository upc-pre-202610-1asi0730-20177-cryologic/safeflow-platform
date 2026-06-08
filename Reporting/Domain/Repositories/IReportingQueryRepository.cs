using SafeFlow.API.Reporting.Domain.Model.Aggregates;

namespace SafeFlow.API.Reporting.Domain.Repositories;

public interface IReportingQueryRepository
{
    Task<IReadOnlyList<ReportCatalogItem>> ListCatalogAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReportRun>> ListRecentRunsAsync(int take, CancellationToken ct = default);
    Task<int> CountDispatchesAsync(CancellationToken ct = default);
    Task<int> CountProductsAsync(CancellationToken ct = default);
    Task<int> CountTemperatureReadingsAsync(CancellationToken ct = default);
    Task<int> CountAlertsAsync(CancellationToken ct = default);
}
