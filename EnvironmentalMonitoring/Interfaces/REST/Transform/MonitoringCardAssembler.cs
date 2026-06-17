using SafeFlow.API.EnvironmentalMonitoring.Application.Services;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Domain.Model;
using SafeFlow.API.Shared.Domain.Services;

namespace SafeFlow.API.EnvironmentalMonitoring.Interfaces.REST.Transform;

public static class MonitoringCardAssembler
{
    public static IReadOnlyList<MonitorCardResult> Build(
        IReadOnlyList<LogisticsDispatch> dispatches,
        IReadOnlyList<LogisticsDriver> drivers,
        IReadOnlyList<LogisticsRoute> routes,
        IReadOnlyList<InventoryLine> inventoryLines,
        IReadOnlyList<TemperatureReading> readings)
    {
        var byDriver = drivers.ToDictionary(d => d.DriverCode);
        var byRoute = routes.ToDictionary(r => r.RouteCode);
        var byLine = inventoryLines.ToDictionary(l => l.LineCode.Value);
        var latestByProduct = readings
            .GroupBy(r => r.ProductCode)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.RecordedAt).First());

        return dispatches
            .OrderByDescending(d => d.DepartureAt)
            .Select(d => BuildCard(d, byDriver, byRoute, byLine, latestByProduct))
            .Where(c => c != null)
            .Cast<MonitorCardResult>()
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();
    }

    public static IReadOnlyList<object> BuildKpis(
        IReadOnlyList<LogisticsDispatch> dispatches,
        IReadOnlyList<MonitorCardResult> monitorCards)
    {
        var total = dispatches.Count;
        var completed = dispatches.Count(d => d.Status == "entregado");
        var transit = dispatches.Count(d => d.Status == "en_transito");
        var outOfRange = monitorCards.Count(c => c.Status == "warning");

        return
        [
            new { id = "shipments", value = total, trendPct = 0, trendUp = true, trendTone = "positive", tone = "blue", icon = "package" },
            new { id = "completed", value = completed, trendPct = 0, trendUp = true, trendTone = "positive", tone = "green", icon = "check" },
            new { id = "transit", value = transit, trendPct = 0, trendUp = false, trendTone = "negative", tone = "amber", icon = "truck" },
            new
            {
                id = "delayed", value = outOfRange, trendPct = 0, trendUp = outOfRange > 0,
                trendTone = outOfRange > 0 ? "negative" : "positive", tone = "rose", icon = "alert"
            }
        ];
    }

    public static object ToApiObject(MonitorCardResult card) => new
    {
        id = card.Id,
        shipmentId = card.ShipmentId,
        idProducto = card.IdProducto,
        idInventario = card.IdInventario,
        titleKey = card.TitleKey,
        productNombre = card.ProductNombre,
        currentTemp = card.CurrentTemp,
        rangeMin = card.RangeMin,
        rangeMax = card.RangeMax,
        status = card.Status,
        personLoc = card.PersonLoc,
        staffRole = card.StaffRole,
        placementKind = card.PlacementKind,
        routeDestinationLoc = card.RouteDestinationLoc,
        warehouseSpotLoc = card.WarehouseSpotLoc
    };

    private static MonitorCardResult? BuildCard(
        LogisticsDispatch dispatch,
        IReadOnlyDictionary<string, LogisticsDriver> byDriver,
        IReadOnlyDictionary<string, LogisticsRoute> byRoute,
        IReadOnlyDictionary<string, InventoryLine> byLine,
        IReadOnlyDictionary<string, TemperatureReading> latestByProduct)
    {
        if (!byLine.TryGetValue(dispatch.InventoryLineCode, out var line))
            return null;

        var product = line.Product;
        if (product == null)
            return null;

        var productCode = product.ProductCode.Value;
        var rangeMin = product.TemperatureMin;
        var rangeMax = product.TemperatureMax;
        latestByProduct.TryGetValue(productCode, out var reading);

        decimal temperature;
        if (reading != null)
            temperature = reading.Temperature;
        else if (dispatch.CurrentTemperature.HasValue)
            temperature = dispatch.CurrentTemperature.Value;
        else
            temperature = (rangeMin + rangeMax) / 2;

        var status = ThermalRange.StatusFromReading(temperature, rangeMin, rangeMax);
        byRoute.TryGetValue(dispatch.RouteCode, out var route);
        byDriver.TryGetValue(dispatch.DriverCode ?? string.Empty, out var driver);

        var routeFrom = LocalizedText.FromRaw(route?.OriginJson);
        var routeTo = LocalizedText.FromRaw(route?.DestinationJson);
        var productName = LocalizedText.FromRaw(product.Name);
        var placementKind = dispatch.PlacementMode == "almacen" ? "warehouse" : "route";
        var personLoc = driver != null ? LocalizedText.FromRaw(driver.NameJson).ToApiObject() : null;
        var staffRole = !string.IsNullOrEmpty(dispatch.DriverCode)
            ? "conductor"
            : !string.IsNullOrEmpty(dispatch.OperatorCode) ? "operario" : "none";

        object? routeDestinationLoc = placementKind == "route" ? routeTo.ToApiObject() : null;
        object? warehouseSpotLoc = placementKind == "warehouse"
            ? LocalizedText.FromRaw(dispatch.WarehouseLocationJson).ToApiObject()
            : routeFrom.ToApiObject();

        if (warehouseSpotLoc == null ||
            (warehouseSpotLoc is { } wh &&
             wh.GetType().GetProperty("en")?.GetValue(wh)?.ToString() == "—"))
            warehouseSpotLoc = LocalizedText.FromRaw(line.Location).ToApiObject();

        return new MonitorCardResult(
            Id: $"ship-{dispatch.DispatchCode}",
            ShipmentId: dispatch.DispatchCode,
            IdProducto: productCode,
            IdInventario: line.LineCode.Value,
            TitleKey: $"bounded.environmental.monitorPoints.{productCode}",
            ProductNombre: productName.ToApiObject(),
            CurrentTemp: temperature,
            RangeMin: rangeMin,
            RangeMax: rangeMax,
            Status: status,
            PersonLoc: personLoc,
            StaffRole: staffRole,
            PlacementKind: placementKind,
            RouteDestinationLoc: routeDestinationLoc,
            WarehouseSpotLoc: warehouseSpotLoc);
    }
}
