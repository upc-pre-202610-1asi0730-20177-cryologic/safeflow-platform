namespace SafeFlow.API.EnvironmentalMonitoring.Application.Services;

public sealed record MonitorCardResult(
    string Id,
    string? ShipmentId,
    string? IdProducto,
    string? IdInventario,
    string? TitleKey,
    object? ProductNombre,
    decimal CurrentTemp,
    decimal RangeMin,
    decimal RangeMax,
    string Status,
    object? PersonLoc,
    string StaffRole,
    string? PlacementKind,
    object? RouteDestinationLoc,
    object? WarehouseSpotLoc);

public sealed record MonitoringDashboardResult(
    IReadOnlyList<object> Kpis,
    IReadOnlyList<MonitorCardResult> MonitorCards);

public interface IEnvironmentalMonitoringQueryService
{
    Task<MonitoringDashboardResult> GetDashboardAsync(CancellationToken ct = default);
    Task<object> ListRegistrosAsync(CancellationToken ct = default);
}
