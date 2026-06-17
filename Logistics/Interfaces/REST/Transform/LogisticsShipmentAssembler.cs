using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Logistics.Interfaces.REST.Transform;

/// <summary>
/// Assembler for transforming Logistics dispatches into enriched shipment API resources.
/// </summary>
/// <remarks>
/// Performs multi-aggregate joins to build comprehensive shipment view combining dispatch metadata,
/// inventory data, carrier/driver information, and route details with localized text and status mappings.
/// </remarks>
public static class LogisticsShipmentAssembler
{
    /// <summary>
    /// Builds a collection of enriched shipment resources by joining multiple domain aggregates.
    /// </summary>
    /// <remarks>
    /// <strong>Joining strategy:</strong> Creates in-memory dictionaries keyed by code to enable O(1) lookup.
    /// 
    /// <strong>Business logic:</strong>
    /// - Quantity is taken from dispatch if positive; otherwise falls back to inventory line quantity.
    /// - Product label combines localized product name with quantity.
    /// - Carrier name prefers the assigned primary driver over the carrier entity.
    /// - Status is mapped to UI representation: "en_transito" → "transit", "entregado" → "delivered", else "pending".
    /// - Thermal risk is determined by the "en_riesgo" status (maps to "risk" or "safe").
    /// - Route origin/destination sourced from route aggregate; current place derived from dispatch status.
    /// </remarks>
    /// <param name="dispatches">Collection of logistics dispatches (primary entity for shipment).</param>
    /// <param name="carriers">Collection of carriers for vehicle type and name resolution.</param>
    /// <param name="drivers">Collection of drivers for primary driver name lookup.</param>
    /// <param name="routes">Collection of routes for origin and destination information.</param>
    /// <param name="inventoryLines">Collection of inventory lines for product and fallback quantity.</param>
    /// <returns>
    /// Read-only list of anonymous objects with properties:
    /// <list type="bullet">
    /// <item><description><c>id</c>: Dispatch code (unique shipment identifier)</description></item>
    /// <item><description><c>fechaSalida</c>: Scheduled departure timestamp</description></item>
    /// <item><description><c>idProducto</c>: Product code from inventory</description></item>
    /// <item><description><c>product</c>: Localized product name with quantity label</description></item>
    /// <item><description><c>carrier</c>: Localized carrier or driver name</description></item>
    /// <item><description><c>providerLine</c>: Carrier name + vehicle type (localized)</description></item>
    /// <item><description><c>routeFrom</c>: Localized origin location</description></item>
    /// <item><description><c>routeTo</c>: Localized destination location</description></item>
    /// <item><description><c>placementKind</c>: "warehouse" or "route" based on placement mode</description></item>
    /// <item><description><c>status</c>: UI status ("transit", "delivered", or "pending")</description></item>
    /// <item><description><c>thermal</c>: Thermal risk classification ("risk" or "safe")</description></item>
    /// <item><description><c>currentTemp</c>: Current temperature reading</description></item>
    /// <item><description><c>originPlace</c>: Localized route origin</description></item>
    /// <item><description><c>originTime</c>: Localized "Scheduled departure" label</description></item>
    /// <item><description><c>currentPlace</c>: Localized current status label ("Awaiting loading" or "In transit")</description></item>
    /// <item><description><c>destPlace</c>: Localized destination</description></item>
    /// <item><description><c>destTime</c>: Localized "Est. per route — arrival" label</description></item>
    /// </list>
    /// </returns>
    public static IReadOnlyList<object> Build(
        IReadOnlyList<LogisticsDispatch> dispatches,
        IReadOnlyList<LogisticsCarrier> carriers,
        IReadOnlyList<LogisticsDriver> drivers,
        IReadOnlyList<LogisticsRoute> routes,
        IReadOnlyList<InventoryLine> inventoryLines)
    {
        var byCarrier = carriers.ToDictionary(c => c.CarrierCode);
        var byDriver = drivers.ToDictionary(d => d.DriverCode);
        var byRoute = routes.ToDictionary(r => r.RouteCode);
        var byLine = inventoryLines.ToDictionary(l => l.LineCode.Value);

        return dispatches.Select(d =>
        {
            byCarrier.TryGetValue(d.CarrierCode, out var carrier);
            byRoute.TryGetValue(d.RouteCode, out var route);
            byLine.TryGetValue(d.InventoryLineCode, out var line);
            var product = line?.Product;
            var productName = LocalizedText.FromRaw(product?.Name);
            var qty = d.Quantity > 0 ? d.Quantity : line?.Quantity ?? 0;
            var productLabel = new LocalizedText(
                $"{productName.En} ({qty} units)",
                $"{productName.Es} ({qty} unidades)");

            LogisticsDriver? primary = null;
            if (!string.IsNullOrEmpty(d.DriverCode)) byDriver.TryGetValue(d.DriverCode, out primary);
            var carrierName = primary != null
                ? LocalizedText.FromRaw(primary.NameJson)
                : LocalizedText.FromRaw(carrier?.NameJson);

            var uiStatus = d.Status switch
            {
                "en_transito" => "transit",
                "entregado" => "delivered",
                _ => "pending"
            };
            var thermal = d.ThermalStatus == "en_riesgo" ? "risk" : "safe";
            var routeFrom = LocalizedText.FromRaw(route?.OriginJson);
            var routeTo = LocalizedText.FromRaw(route?.DestinationJson);

            return new
            {
                id = d.DispatchCode,
                fechaSalida = d.DepartureAt,
                idProducto = product?.ProductCode.Value,
                product = productLabel.ToApiObject(),
                carrier = carrierName.ToApiObject(),
                providerLine = new LocalizedText(
                    $"{carrierName.En} • {LocalizedText.FromRaw(carrier?.VehicleTypeJson).En}",
                    $"{carrierName.Es} • {LocalizedText.FromRaw(carrier?.VehicleTypeJson).Es}").ToApiObject(),
                routeFrom = routeFrom.ToApiObject(),
                routeTo = routeTo.ToApiObject(),
                placementKind = d.PlacementMode == "almacen" ? "warehouse" : "route",
                status = uiStatus,
                thermal,
                currentTemp = d.CurrentTemperature,
                originPlace = routeFrom.ToApiObject(),
                originTime = new LocalizedText("Scheduled departure", "Salida programada").ToApiObject(),
                currentPlace = uiStatus == "pending"
                    ? new LocalizedText("Awaiting loading", "En espera de carga").ToApiObject()
                    : new LocalizedText("In transit", "En tránsito").ToApiObject(),
                destPlace = routeTo.ToApiObject(),
                destTime = new LocalizedText("Est. per route — arrival", "Est. según ruta — llegada").ToApiObject()
            };
        }).ToList();
    }
}
