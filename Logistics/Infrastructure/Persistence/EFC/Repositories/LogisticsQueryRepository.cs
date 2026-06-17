using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Logistics.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Logistics.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// EF Core implementation of the read-only query repository for logistics domain.
/// Provides optimized, tracking-free queries for all logistics aggregates.
/// </summary>
public class LogisticsQueryRepository(AppDbContext context) : ILogisticsQueryRepository
{
    /// <summary>
    /// Retrieves all logistics dispatches as a read-only list.
    /// </summary>
    public async Task<IReadOnlyList<LogisticsDispatch>> ListDispatchesAsync(CancellationToken ct = default)
        => await context.LogisticsDispatches
            .AsNoTracking()
            .ToListAsync(ct);

    /// <summary>
    /// Retrieves all logistics drivers as a read-only list.
    /// </summary>
    public async Task<IReadOnlyList<LogisticsDriver>> ListDriversAsync(CancellationToken ct = default)
        => await context.LogisticsDrivers
            .AsNoTracking()
            .ToListAsync(ct);

    /// <summary>
    /// Retrieves all logistics destinations as a read-only list.
    /// </summary>
    public async Task<IReadOnlyList<LogisticsDestination>> ListDestinationsAsync(CancellationToken ct = default)
        => await context.LogisticsDestinations
            .AsNoTracking()
            .ToListAsync(ct);

    /// <summary>
    /// Retrieves all logistics routes as a read-only list.
    /// </summary>
    public async Task<IReadOnlyList<LogisticsRoute>> ListRoutesAsync(CancellationToken ct = default)
        => await context.LogisticsRoutes
            .AsNoTracking()
            .ToListAsync(ct);

    /// <summary>
    /// Retrieves all logistics carriers as a read-only list.
    /// </summary>
    public async Task<IReadOnlyList<LogisticsCarrier>> ListCarriersAsync(CancellationToken ct = default)
        => await context.LogisticsCarriers
            .AsNoTracking()
            .ToListAsync(ct);
}