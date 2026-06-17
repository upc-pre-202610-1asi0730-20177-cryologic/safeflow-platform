using SafeFlow.API.Alerts.Application.Services;
using SafeFlow.API.EnvironmentalMonitoring.Application.Services;
using SafeFlow.API.Inventory.Application.Services;
using SafeFlow.API.Inventory.Domain.Model.Queries;
using SafeFlow.API.Inventory.Interfaces.REST.Transform;
using SafeFlow.API.Logistics.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Inventory.Interfaces.REST;

[ApiController]
[Route("api/inventory")]
public class InventorySnapshotController(
    IInventoryQueryService inventoryQuery,
    ILogisticsQueryService logisticsQuery,
    IEnvironmentalMonitoringQueryService monitoringQuery,
    IAlertQueryService alertQuery) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSnapshot(CancellationToken ct)
    {
        var items = await inventoryQuery.Handle(new GetAllInventoryItemsQuery(), ct);
        var logistics = await logisticsQuery.GetRawSnapshotAsync(ct);
        var registros = await monitoringQuery.ListRegistrosAsync(ct);
        var alertas = await alertQuery.ListAlertasAsync(ct);
        var monitoring = await monitoringQuery.GetDashboardAsync(ct);
        var atRisk = monitoring.MonitorCards.Count(c => c.Status == "warning");

        return Ok(new
        {
            inventory = new
            {
                productos = items.Select(i => new
                {
                    idProducto = i.IdProducto,
                    nombre = i.Name,
                    categoria = i.Category,
                    temperaturaMin = i.TemperaturaMin,
                    temperaturaMax = i.TemperaturaMax,
                    fechaVencimiento = i.FechaVencimiento,
                    lote = i.Lote,
                    estado = InventoryItemResourceAssembler.UiStatusToDomain(i.Status)
                }),
                inventario = items.Select(i => new
                {
                    idInventario = i.IdInventario,
                    idProducto = i.IdProducto,
                    cantidad = i.Qty,
                    ubicacion = i.Location,
                    fechaIngreso = i.FechaIngreso
                })
            },
            logistics,
            environmentalMonitoring = registros,
            alerts = alertas,
            system = new
            {
                indicadores = new { productosEnRiesgo = atRisk }
            }
        });
    }
}
