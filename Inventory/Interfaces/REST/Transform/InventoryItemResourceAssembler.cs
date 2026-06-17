using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Interfaces.REST.Resources;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Inventory.Interfaces.REST.Transform;

public static class InventoryItemResourceAssembler
{
    private static readonly Dictionary<string, string> UiStatusByDomain = new(StringComparer.OrdinalIgnoreCase)
    {
        ["disponible"] = "available",
        ["en_riesgo"] = "risk",
        ["desechado"] = "discarded",
        ["en_transito"] = "in_transit"
    };

    private static readonly Dictionary<string, string> DomainStatusByUi = new(StringComparer.OrdinalIgnoreCase)
    {
        ["available"] = "disponible",
        ["risk"] = "en_riesgo",
        ["inactive"] = "desechado",
        ["discarded"] = "desechado",
        ["in_transit"] = "en_transito"
    };

    private static readonly Dictionary<string, string> LocationSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["main"] = "t:places.main_warehouse",
        ["freezer1"] = "t:places.freezer1",
        ["coldRoom"] = "t:places.coldRoom",
        ["sectorB"] = "t:places.sectorB",
        ["almacen_a"] = "t:places.almacen_a",
        ["almacen_b"] = "t:places.almacen_b"
    };

    public static string UiStatusToDomain(string? ui) =>
        ui != null && DomainStatusByUi.TryGetValue(ui, out var d) ? d : "disponible";

    public static string DomainStatusToUi(string domain) =>
        UiStatusByDomain.TryGetValue(domain, out var ui) ? ui : "available";

    public static string ResolveLocation(string? slugOrKey)
    {
        if (string.IsNullOrWhiteSpace(slugOrKey)) return "t:places.main_warehouse";
        var key = slugOrKey.Trim();
        return LocationSlugs.TryGetValue(key, out var stored) ? stored : key;
    }

    public static InventoryItemResource ToResource(InventoryLine line)
    {
        var product = line.Product;
        var name = LocalizedText.FromRaw(product.Name);
        var category = LocalizedText.FromRaw(product.Category);
        var location = LocalizedText.FromRaw(line.Location);
        var tempLabel = BuildTempLabel(product.TemperatureMin, product.TemperatureMax);
        return new InventoryItemResource(
            Id: line.LineCode.Value,
            IdProducto: product.ProductCode.Value,
            IdInventario: line.LineCode.Value,
            Qty: line.Quantity,
            Status: DomainStatusToUi(product.Status),
            TempTone: BuildTempTone(product.TemperatureMin, product.TemperatureMax),
            Name: name.ToApiObject(),
            Category: category.ToApiObject(),
            TempLabel: tempLabel.ToApiObject(),
            Location: location.ToApiObject(),
            TemperaturaMin: product.TemperatureMin,
            TemperaturaMax: product.TemperatureMax,
            Lote: product.Batch,
            FechaVencimiento: product.ExpiryDate?.ToString("yyyy-MM-dd"),
            FechaIngreso: line.EntryDate.ToString("yyyy-MM-dd"));
    }

    private static LocalizedText BuildTempLabel(decimal min, decimal max)
    {
        return new LocalizedText($"{min}°C to {max}°C", $"{min}°C a {max}°C");
    }

    private static string BuildTempTone(decimal min, decimal max)
    {
        if (max <= 0 || min < -10) return "frozen";
        return "chilled";
    }
}
