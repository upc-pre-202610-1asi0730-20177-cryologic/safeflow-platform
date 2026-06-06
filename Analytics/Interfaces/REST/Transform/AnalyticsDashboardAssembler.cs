using System.Text.Json;
using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Analytics.Interfaces.REST.Transform;

public static class AnalyticsDashboardAssembler
{
    public static object Build(
        object shipmentsPayload,
        object choferesPayload,
        IReadOnlyList<LogisticsDriver> drivers,
        IReadOnlyList<LogisticsDispatch> dispatches,
        IReadOnlyList<InventoryLine> lines)
    {
        var shipments = ExtractShipments(shipmentsPayload);
        var choferes = ExtractChoferes(choferesPayload);

        var total = shipments.Count;
        var completed = shipments.Count(s => GetString(s, "status") == "delivered");
        var transit = shipments.Count(s => GetString(s, "status") == "transit");
        var delayed = shipments.Count(s =>
            GetString(s, "status") != "delivered" && GetString(s, "thermal") == "risk");

        var recent = shipments
            .OrderByDescending(s => GetString(s, "fechaSalida"))
            .Take(5)
            .Select(s => new
            {
                id = $"s-{GetString(s, "id")}",
                trackingId = GetString(s, "id"),
                status = GetString(s, "status"),
                destination = GetObject(s, "routeTo") ?? new { en = "—", es = "—" },
                carrier = GetObject(s, "carrier") ?? new { en = "—", es = "—" },
                dateIso = GetString(s, "fechaSalida")
            });

        return new
        {
            kpis = new object[]
            {
                Kpi("shipments", total, "blue", "package"),
                Kpi("completed", completed, "green", "check"),
                Kpi("transit", transit, "amber", "truck", false),
                Kpi("delayed", delayed, "rose", "alert", delayed > 0)
            },
            deliveryMix = new
            {
                deliveredPct = Pct(shipments, s => GetString(s, "status") == "delivered", total),
                transitPct = Pct(shipments, s => GetString(s, "status") == "transit" && GetString(s, "thermal") != "risk", total),
                pendingPct = Pct(shipments, s => GetString(s, "status") == "pending" && GetString(s, "thermal") != "risk", total),
                delayedPct = Pct(shipments, s => GetString(s, "status") != "delivered" && GetString(s, "thermal") == "risk", total)
            },
            recentShipments = recent,
            fleet = BuildFleet(shipments, choferes, drivers),
            chartWeek = BuildWeekChart(dispatches),
            chartMonth = BuildMonthChart(dispatches)
        };
    }

    private static List<JsonElement> ExtractShipments(object payload)
    {
        var json = JsonSerializer.SerializeToElement(payload);
        if (json.TryGetProperty("shipments", out var arr) && arr.ValueKind == JsonValueKind.Array)
            return arr.EnumerateArray().ToList();
        return [];
    }

    private static List<JsonElement> ExtractChoferes(object payload)
    {
        var json = JsonSerializer.SerializeToElement(payload);
        if (json.TryGetProperty("choferes", out var arr) && arr.ValueKind == JsonValueKind.Array)
            return arr.EnumerateArray().ToList();
        return [];
    }

    private static IReadOnlyList<object> BuildFleet(
        List<JsonElement> shipments,
        List<JsonElement> choferes,
        IReadOnlyList<LogisticsDriver> drivers)
    {
        var transit = shipments.Where(s => GetString(s, "status") == "transit").ToList();
        var onRoute = new HashSet<string>();

        foreach (var driver in drivers.Where(d => d.Role != "operario"))
        {
            var on = transit.Any(s =>
            {
                var carrier = GetString(s, "carrier");
                var name = LocalizedText.FromRaw(driver.NameJson).En;
                return carrier.Contains(name, StringComparison.OrdinalIgnoreCase);
            });
            if (on) onRoute.Add(driver.DriverCode);
        }

        return drivers
            .Where(d => d.Role != "operario")
            .Select(d =>
            {
                var on = onRoute.Contains(d.DriverCode);
                var h = Math.Abs(d.DriverCode.GetHashCode());
                return new
                {
                    id = d.DriverCode,
                    nameLoc = LocalizedText.FromRaw(d.NameJson).ToApiObject(),
                    vehicleCode = d.EmployeeCode,
                    progress = on ? 42 + (h % 48) : 100,
                    status = on ? "on_route" : "available"
                };
            })
            .OrderBy(x => x.status == "on_route" ? 0 : 1)
            .Take(5)
            .Cast<object>()
            .ToList();
    }

    private static IReadOnlyList<object> BuildWeekChart(IReadOnlyList<LogisticsDispatch> dispatches)
    {
        var now = DateTime.UtcNow.Date;
        var list = new List<object>();
        for (var i = 6; i >= 0; i--)
        {
            var day = now.AddDays(-i);
            var next = day.AddDays(1);
            var count = dispatches.Count(d =>
                d.DepartureAt.HasValue &&
                d.DepartureAt.Value.UtcDateTime >= day &&
                d.DepartureAt.Value.UtcDateTime < next);
            list.Add(new { key = day.ToString("yyyy-MM-dd"), value = count });
        }

        return list;
    }

    private static IReadOnlyList<object> BuildMonthChart(IReadOnlyList<LogisticsDispatch> dispatches)
    {
        var counts = dispatches
            .Where(d => d.DepartureAt.HasValue)
            .GroupBy(d => d.DepartureAt!.Value.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .TakeLast(6)
            .Select(g => new { key = g.Key, value = g.Count() })
            .Cast<object>()
            .ToList();
        return counts;
    }

    private static int Pct(List<JsonElement> items, Func<JsonElement, bool> pred, int total)
    {
        if (total == 0) return 0;
        return (int)Math.Round(items.Count(pred) / (double)total * 100);
    }

    private static object Kpi(string id, int value, string tone, string icon, bool negative = false) => new
    {
        id,
        value,
        trendPct = 0,
        trendUp = !negative,
        trendTone = negative ? "negative" : "positive",
        tone,
        icon
    };

    private static string GetString(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) ? p.ToString() : "";

    private static object? GetObject(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Object
            ? JsonSerializer.Deserialize<object>(p.GetRawText())
            : null;
}
