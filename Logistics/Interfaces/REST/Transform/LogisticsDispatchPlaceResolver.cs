using System.Text.Json;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Logistics.Interfaces.REST.Transform;

/// <summary>
/// Resuelve origen/destino de un despacho: prioriza <see cref="LogisticsDispatch.TextsJson"/>
/// (lo elegido al crear el envío) y cae en la ruta o almacén asociados.
/// </summary>
public static class LogisticsDispatchPlaceResolver
{
    public static (LocalizedText Origin, LocalizedText Destination) ResolveRoutePlaces(
        LogisticsDispatch dispatch,
        LogisticsRoute? route)
    {
        var fallbackFrom = LocalizedText.FromRaw(route?.OriginJson);
        var fallbackTo = LocalizedText.FromRaw(route?.DestinationJson);

        if (dispatch.PlacementMode == "almacen")
        {
            var wh = LocalizedText.FromRaw(dispatch.WarehouseLocationJson);
            if (wh.En != "—" || wh.Es != "—")
                return (wh, wh);
        }

        if (string.IsNullOrWhiteSpace(dispatch.TextsJson))
            return (fallbackFrom, fallbackTo);

        try
        {
            using var doc = JsonDocument.Parse(dispatch.TextsJson);
            var root = doc.RootElement;
            var origin = root.TryGetProperty("originPlace", out var op)
                ? FromJsonElement(op, fallbackFrom)
                : fallbackFrom;
            var dest = root.TryGetProperty("destPlace", out var dp)
                ? FromJsonElement(dp, fallbackTo)
                : fallbackTo;
            return (origin, dest);
        }
        catch
        {
            return (fallbackFrom, fallbackTo);
        }
    }

    private static LocalizedText FromJsonElement(JsonElement el, LocalizedText fallback)
    {
        if (el.ValueKind != JsonValueKind.Object)
            return fallback;

        var en = el.TryGetProperty("en", out var e) ? e.GetString()?.Trim() : null;
        var es = el.TryGetProperty("es", out var s) ? s.GetString()?.Trim() : null;

        if (string.IsNullOrWhiteSpace(en) && string.IsNullOrWhiteSpace(es))
            return fallback;

        return new LocalizedText(
            string.IsNullOrWhiteSpace(en) ? es! : en!,
            string.IsNullOrWhiteSpace(es) ? en! : es!);
    }
}
