using SafeFlow.API.Logistics.Domain.Model.Aggregates;

namespace SafeFlow.API.Logistics.Domain.Repositories;

/// <summary>
/// Read-only repository interface for logistics aggregates.
/// Provides optimized query access without tracking.
/// </summary>
public interface ILogisticsQueryRepository
{
    /// <summary>Gets all dispatches.</summary>
    Task<IReadOnlyList<LogisticsDispatch>> ListDispatchesAsync(CancellationToken ct = default);

    /// <summary>Gets all drivers.</summary>
    Task<IReadOnlyList<LogisticsDriver>> ListDriversAsync(CancellationToken ct = default);

    /// <summary>Gets all destinations.</summary>
    Task<IReadOnlyList<LogisticsDestination>> ListDestinationsAsync(CancellationToken ct = default);

    /// <summary>Gets all routes.</summary>
    Task<IReadOnlyList<LogisticsRoute>> ListRoutesAsync(CancellationToken ct = default);

    /// <summary>Gets all carriers.</summary>
    Task<IReadOnlyList<LogisticsCarrier>> ListCarriersAsync(CancellationToken ct = default);
}