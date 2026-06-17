using System.Text.Json;
using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Logistics.Application.Services;
using SafeFlow.API.Logistics.Domain.Repositories;
using SafeFlow.API.Logistics.Interfaces.REST.Transform;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Logistics.Application.Internal.QueryServices;

/// <summary>
/// Service providing read-only query operations for logistics data,
/// retrieving structured snapshots of shipments, drivers, and destinations.
/// </summary>
public class LogisticsQueryService(
    ILogisticsQueryRepository logisticsRepository,
    IInventoryLineRepository inventoryLineRepository) : ILogisticsQueryService
{
    /// <summary>
    /// Retrieves a unified list of all active shipments aggregated with their related context.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An anonymous object containing an array of assembled shipments.</returns>
    public async Task<object> ListShipmentsAsync(CancellationToken ct = default)
    {
        var dispatches = await logisticsRepository.ListDispatchesAsync(ct);
        if (dispatches.Count == 0)
            return new { shipments = Array.Empty<object>() };

        // Fetch dependent tracking dimensions for full aggregate compilation
        var carriers = await logisticsRepository.ListCarriersAsync(ct);
        var drivers = await logisticsRepository.ListDriversAsync(ct);
        var routes = await logisticsRepository.ListRoutesAsync(ct);
        var lines = await inventoryLineRepository.ListWithProductsAsync(ct);

        var shipments = LogisticsShipmentAssembler.Build(
            dispatches, carriers, drivers, routes, lines);
        return new { shipments };
    }

    /// <summary>
    /// Retrieves a list of all defined logistics destinations.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An anonymous collection representing mapped destinations with localized text structures.</returns>
    public async Task<object> ListDestinosAsync(CancellationToken ct = default)
    {
        var destinos = await logisticsRepository.ListDestinationsAsync(ct);
        return new
        {
            destinos = destinos.Select(d => new
            {
                idDestino = d.DestinationCode,
                codigo = d.Slug,
                nombre = ParseJson(d.NameJson).ToApiObject()
            })
        };
    }

    /// <summary>
    /// Retrieves a list of all registered drivers and operators.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An anonymous collection mapping individual carrier personnel profiles.</returns>
    public async Task<object> ListChoferesAsync(CancellationToken ct = default)
    {
        var choferes = await logisticsRepository.ListDriversAsync(ct);
        return new
        {
            choferes = choferes.Select(c => new
            {
                idChofer = c.DriverCode,
                idTransportista = c.CarrierCode,
                codigo = c.EmployeeCode,
                nombre = ParseJson(c.NameJson).ToApiObject(),
                licencia = c.License,
                contacto = c.Contact,
                rol = c.Role
            })
        };
    }

    /// <summary>
    /// Computes a comprehensive, multi-entity snapshot of the entire logistics data boundary.
    /// Used for dashboard synchronization or system auditing.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary-style anonymous structure compiling carriers, drivers, routes, and dispatches.</returns>
    public async Task<object> GetRawSnapshotAsync(CancellationToken ct = default)
    {
        var carriers = await logisticsRepository.ListCarriersAsync(ct);
        var drivers = await logisticsRepository.ListDriversAsync(ct);
        var destinos = await logisticsRepository.ListDestinationsAsync(ct);
        var routes = await logisticsRepository.ListRoutesAsync(ct);
        var despachos = await logisticsRepository.ListDispatchesAsync(ct);

        return new
        {
            transportistas = carriers.Select(c => new
            {
                idTransportista = c.CarrierCode,
                nombre = ParseJson(c.NameJson).ToApiObject(),
                tipoVehiculo = ParseJson(c.VehicleTypeJson).ToApiObject(),
                contacto = c.Contact,
                codigo = c.FleetCode
            }),
            choferes = drivers.Select(c => new
            {
                idChofer = c.DriverCode,
                idTransportista = c.CarrierCode,
                codigo = c.EmployeeCode,
                nombre = ParseJson(c.NameJson).ToApiObject(),
                licencia = c.License,
                contacto = c.Contact,
                rol = c.Role
            }),
            destinos = destinos.Select(d => new
            {
                idDestino = d.DestinationCode,
                codigo = d.Slug,
                nombre = ParseJson(d.NameJson).ToApiObject()
            }),
            rutas = routes.Select(r => new
            {
                idRuta = r.RouteCode,
                codigo = r.Slug,
                origen = ParseJson(r.OriginJson).ToApiObject(),
                destino = ParseJson(r.DestinationJson).ToApiObject()
            }),
            despachos = despachos.Select(d => new
            {
                idDespacho = d.DispatchCode,
                idTransportista = d.CarrierCode,
                idChofer = d.DriverCode,
                idOperario = d.OperatorCode,
                idRuta = d.RouteCode,
                idInventario = d.InventoryLineCode,
                cantidad = d.Quantity,
                estado = d.Status,
                estadoTermico = d.ThermalStatus,
                temperaturaActual = d.CurrentTemperature,
                fechaSalida = d.DepartureAt,
                fechaEntregaEstimada = d.EstimatedArrivalAt,
                modoUbicacion = d.PlacementMode,
                // Optional conditional translation block depending on location strategies
                ubicacionAlmacen = d.WarehouseLocationJson != null
                    ? ParseJson(d.WarehouseLocationJson).ToApiObject()
                    : null
            })
        };
    }

    // Unpacks raw database JSON strings back into strongly-typed local domain schemas
    private static LocalizedText ParseJson(string json) => LocalizedText.FromRaw(json);
}