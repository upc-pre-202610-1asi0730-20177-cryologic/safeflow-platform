using SafeFlow.API.Logistics.Domain.Model.Aggregates;
namespace SafeFlow.API.Logistics.Domain.Repositories;

public interface ILogisticsQueryRepository
{
    Task<IReadOnlyList<LogisticsDispatch>> ListDispatchesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LogisticsDriver>> ListDriversAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LogisticsDestination>> ListDestinationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LogisticsRoute>> ListRoutesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LogisticsCarrier>> ListCarriersAsync(CancellationToken ct = default);
}