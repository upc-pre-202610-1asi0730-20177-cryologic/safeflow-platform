using SafeFlow.API.Reporting.Application.Services;
using SafeFlow.API.Reporting.Domain.Repositories;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Reporting.Application.Internal;

public class ReportingQueryService(IReportingQueryRepository repository) : IReportingQueryService
{
    public async Task<object> GetDashboardAsync(CancellationToken ct = default)
    {
        var despachos = await repository.CountDispatchesAsync(ct);
        var productos = await repository.CountProductsAsync(ct);
        var registros = await repository.CountTemperatureReadingsAsync(ct);
        var alertas = await repository.CountAlertsAsync(ct);
        var catalog = await repository.ListCatalogAsync(ct);
        var runs = await repository.ListRecentRunsAsync(10, ct);

        return new
        {
            stats = new object[]
            {
                new { id = "a1", valueKind = "number", value = despachos },
                new { id = "a2", valueKind = "number", value = productos },
                new { id = "a3", valueKind = "number", value = registros },
                new { id = "a4", valueKind = "number", value = alertas }
            },
            catalog = catalog.Select(c => new
            {
                id = c.CatalogCode,
                format = c.Format,
                title = LocalizedText.FromRaw(c.TitleJson).ToApiObject(),
                description = c.DescriptionJson != null
                    ? LocalizedText.FromRaw(c.DescriptionJson).ToApiObject()
                    : null
            }),
            recentRuns = runs.Select(r => new
            {
                id = r.RunCode,
                format = r.Format,
                status = r.Status,
                generatedAt = r.GeneratedAt
            })
        };
    }
}
