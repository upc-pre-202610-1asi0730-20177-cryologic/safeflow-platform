using SafeFlow.API.Alerts.Application.Errors;
using SafeFlow.API.Alerts.Application.Services;
using SafeFlow.API.Alerts.Domain.Model.Commands;
using SafeFlow.API.Alerts.Domain.Model.Queries;
using SafeFlow.API.Alerts.Domain.Model.ValueObjects;
using SafeFlow.API.Shared.Domain.Model;
using SafeFlow.API.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Alerts.Interfaces.REST;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController(IAlertQueryService queryService, IAlertCommandService commandService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => Ok(await queryService.Handle(new GetAlertsDashboardQuery(), ct));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await queryService.ListAlertasAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAlertRequest body, CancellationToken ct)
    {
        var command = new CreateAlertCommand(
            new AlertCode(body.IdAlerta ?? $"ALT-{Guid.NewGuid().ToString("N")[..6].ToUpper()}"),
            body.Tipo ?? "sistema",
            body.Severidad ?? "high",
            body.Titulo != null
                ? System.Text.Json.JsonSerializer.Serialize(body.Titulo)
                : new LocalizedText("Alert", "Alerta").ToStorageJson(),
            body.Mensaje != null ? System.Text.Json.JsonSerializer.Serialize(body.Mensaje) : null,
            body.IdProducto,
            body.IdDespacho,
            body.FechaHora ?? DateTimeOffset.UtcNow);

        var result = await commandService.Handle(command, ct);
        return result.Fold<IActionResult>(
            alert => Created(string.Empty, new { idAlerta = alert.AlertCode.Value }),
            error => error == AlertCommandError.DuplicateCode
                ? Conflict()
                : StatusCode(500));
    }

    [HttpPatch("{id}/resolve")]
    public async Task<IActionResult> Resolve(string id, CancellationToken ct)
    {
        var result = await commandService.Handle(new ResolveAlertCommand(new AlertCode(id)), ct);
        return result.Fold<IActionResult>(
            _ => NoContent(),
            error => error == AlertCommandError.NotFound ? NotFound() : StatusCode(500));
    }
}

public record CreateAlertRequest(
    string? IdAlerta,
    string? Tipo,
    string? Severidad,
    object? Titulo,
    object? Mensaje,
    string? IdProducto,
    string? IdDespacho,
    DateTimeOffset? FechaHora);
