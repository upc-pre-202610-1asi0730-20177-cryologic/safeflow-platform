using SafeFlow.API.EnvironmentalMonitoring.Application.Services;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Repositories;
using SafeFlow.API.EnvironmentalMonitoring.Interfaces.REST.Transform;
using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Logistics.Domain.Repositories;

namespace SafeFlow.API.EnvironmentalMonitoring.Application.Internal;

public class EnvironmentalMonitoringQueryService(
    ITemperatureReadingRepository readingRepository,
    ILogisticsQueryRepository logisticsRepository,
    IInventoryLineRepository inventoryLineRepository) : IEnvironmentalMonitoringQueryService
{
    public async Task<MonitoringDashboardResult> GetDashboardAsync(CancellationToken ct = default)
    {
        var readings = await readingRepository.ListOrderedAsync(ct);
        var dispatches = await logisticsRepository.ListDispatchesAsync(ct);
        var drivers = await logisticsRepository.ListDriversAsync(ct);
        var routes = await logisticsRepository.ListRoutesAsync(ct);
        var lines = await inventoryLineRepository.ListWithProductsAsync(ct);

        var monitorCards = MonitoringCardAssembler.Build(dispatches, drivers, routes, lines, readings);
        var kpis = MonitoringCardAssembler.BuildKpis(dispatches, monitorCards);

        return new MonitoringDashboardResult(kpis, monitorCards);
    }

    public async Task<object> ListRegistrosAsync(CancellationToken ct = default)
    {
        var readings = await readingRepository.ListOrderedAsync(ct);
        return new
        {
            registrosTemperatura = readings.Select(r => new
            {
                idRegistro = r.ReadingCode.Value,
                idProducto = r.ProductCode,
                temperatura = r.Temperature,
                fechaHora = r.RecordedAt,
                fuente = r.Source
            })
        };
    }
}
