namespace SafeFlow.API.Logistics.Application.Services;

/// <summary>
/// Defines the read-only query service contract for fetching logistics snapshots,
/// compiling material tracking entities, personnel data, and distribution routes.
/// </summary>
public interface ILogisticsQueryService
{
    /// <summary>
    /// Accumulates and assemblies all active product shipment dispatches into a tracking collection view.
    /// </summary>
    Task<object> ListShipmentsAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a list of all configured cargo and transfer destinations.
    /// </summary>
    Task<object> ListDestinosAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a list of active transit profiles representing field drivers and operators.
    /// </summary>
    Task<object> ListChoferesAsync(CancellationToken ct = default);

    /// <summary>
    /// Generates a comprehensive snapshot mapping carriers, routes, personnel, and dispatches across the system boundary.
    /// </summary>
    Task<object> GetRawSnapshotAsync(CancellationToken ct = default);
}