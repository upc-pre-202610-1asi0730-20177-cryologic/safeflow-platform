using SafeFlow.API.Reporting.Domain.Model.Aggregates;
using SafeFlow.API.Reporting.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Reporting.Infrastructure.Persistence.EFC.Repositories;

public class ReportingQueryRepository(AppDbContext context) : IReportingQueryRepository
{
    public async Task<IReadOnlyList<ReportCatalogItem>> ListCatalogAsync(CancellationToken ct = default)
        => await context.ReportCatalogItems.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<ReportRun>> ListRecentRunsAsync(int take, CancellationToken ct = default)
        => await context.ReportRuns.AsNoTracking()
            .OrderByDescending(r => r.GeneratedAt)
            .Take(take)
            .ToListAsync(ct);

    public Task<int> CountDispatchesAsync(CancellationToken ct = default)
        => context.LogisticsDispatches.CountAsync(ct);

    public Task<int> CountProductsAsync(CancellationToken ct = default)
        => context.Products.CountAsync(ct);

    public Task<int> CountTemperatureReadingsAsync(CancellationToken ct = default)
        => context.TemperatureReadings.CountAsync(ct);

    public Task<int> CountAlertsAsync(CancellationToken ct = default)
        => context.Alerts.CountAsync(ct);
}
