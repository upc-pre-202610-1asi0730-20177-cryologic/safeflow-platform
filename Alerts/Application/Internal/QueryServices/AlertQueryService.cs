using SafeFlow.API.Alerts.Domain.Model.Queries;
using SafeFlow.API.Alerts.Domain.Repositories;
using SafeFlow.API.Alerts.Application.Services;
using SafeFlow.API.EnvironmentalMonitoring.Application.Services;
using SafeFlow.API.Shared.Domain.Model;

namespace SafeFlow.API.Alerts.Application.Internal.QueryServices;

public class AlertQueryService(
    IAlertRepository alertRepository,
    IEnvironmentalMonitoringQueryService monitoringQueryService) : IAlertQueryService
{
    public async Task<object> Handle(GetAlertsDashboardQuery query, CancellationToken ct = default)
    {
        var alertas = await alertRepository.ListOrderedAsync(ct);
        var activas = alertas.Count(a => a.Status == "activa");
        var resueltas = alertas.Count(a => a.Status == "resuelta");
        var monitoring = await monitoringQueryService.GetDashboardAsync(ct);
        var atRisk = monitoring.MonitorCards.Count(c => c.Status == "warning");

        return new
        {
            kpis = new object[]
            {
                Kpi("shipments", alertas.Count, "blue", "package", true, 5),
                Kpi("completed", resueltas, "green", "check", true, 3),
                Kpi("transit", activas, "amber", "truck", false, 1.5),
                Kpi("delayed", atRisk, "rose", "alert", true, 0.8)
            },
            feedItems = Array.Empty<object>()
        };
    }

    public async Task<object> ListAlertasAsync(CancellationToken ct = default)
    {
        var alertas = await alertRepository.ListOrderedAsync(ct);
        return new
        {
            alertas = alertas.Select(a => new
            {
                idAlerta = a.AlertCode.Value,
                tipo = a.AlertType,
                estado = a.Status,
                severidad = a.Severity,
                titulo = LocalizedText.FromRaw(a.TitleJson).ToApiObject(),
                mensaje = a.MessageJson != null ? LocalizedText.FromRaw(a.MessageJson).ToApiObject() : null,
                idProducto = a.ProductCode,
                idDespacho = a.DispatchCode,
                fechaHora = a.RecordedAt
            })
        };
    }

    private static object Kpi(
        string id, int value, string tone, string icon, bool trendUp, double trendPct) => new
    {
        id,
        value,
        trendPct,
        trendUp,
        trendTone = trendUp ? "positive" : "negative",
        tone,
        icon
    };
}
