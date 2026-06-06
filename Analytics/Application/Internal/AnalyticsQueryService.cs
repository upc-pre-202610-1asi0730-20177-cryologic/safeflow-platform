using SafeFlow.API.Analytics.Application.Services;
using SafeFlow.API.Analytics.Interfaces.REST.Transform;
using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Logistics.Application.Services;
using SafeFlow.API.Logistics.Domain.Repositories;

namespace SafeFlow.API.Analytics.Application.Internal;

public class AnalyticsQueryService(
    ILogisticsQueryService logisticsQueryService,
    ILogisticsQueryRepository logisticsRepository,
    IInventoryLineRepository inventoryLineRepository) : IAnalyticsQueryService
{
    public async Task<object> GetDashboardAsync(CancellationToken ct = default)
    {
        var shipmentsPayload = await logisticsQueryService.ListShipmentsAsync(ct);
        var choferesPayload = await logisticsQueryService.ListChoferesAsync(ct);
        var drivers = await logisticsRepository.ListDriversAsync(ct);
        var dispatches = await logisticsRepository.ListDispatchesAsync(ct);
        var lines = await inventoryLineRepository.ListWithProductsAsync(ct);

        return AnalyticsDashboardAssembler.Build(
            shipmentsPayload,
            choferesPayload,
            drivers,
            dispatches,
            lines);
    }
}
