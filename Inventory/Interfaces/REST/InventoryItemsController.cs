using SafeFlow.API.Inventory.Application.Errors;
using SafeFlow.API.Inventory.Application.Services;
using SafeFlow.API.Inventory.Domain.Model.Commands;
using SafeFlow.API.Inventory.Domain.Model.Queries;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;
using SafeFlow.API.Inventory.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Inventory.Interfaces.REST;

[ApiController]
[Route("api/inventory/items")]
public class InventoryItemsController(
    IInventoryQueryService queryService,
    IInventoryCommandService commandService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await queryService.Handle(new GetAllInventoryItemsQuery(), ct);
        return Ok(new { items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var item = await queryService.Handle(
            new GetInventoryItemByLineCodeQuery(new InventoryLineCode(id)), ct);
        return item == null ? NotFound(new { error = "Not found" }) : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Name))
            return BadRequest(new { error = "Missing name" });

        var productCode = new ProductCode(NextProductCode());
        var lineCode = new InventoryLineCode(NextLineCode());
        var category = string.IsNullOrWhiteSpace(body.Category) ? "t:productCategory.medicine" : body.Category!;
        var location = InventoryItemResourceAssembler.ResolveLocation(body.Location ?? "main");
        var status = InventoryItemResourceAssembler.UiStatusToDomain(body.Status ?? "available");
        var tempMin = body.TempMin ?? body.TemperaturaMin ?? 2;
        var tempMax = body.TempMax ?? body.TemperaturaMax ?? 8;

        var command = new CreateProductCommand(
            productCode,
            body.Name!,
            category,
            tempMin,
            tempMax,
            ParseDate(body.FechaVencimiento),
            body.Lote ?? $"LOT-{productCode.Value[^3..]}",
            status,
            Math.Max(0, body.Qty ?? 0),
            location,
            ParseDate(body.FechaIngreso) ?? DateOnly.FromDateTime(DateTime.UtcNow),
            lineCode);

        var result = await commandService.Handle(command, ct);
        return result.Fold(
            line => Created(string.Empty, InventoryItemResourceAssembler.ToResource(line)),
            error => error switch
            {
                InventoryCommandError.DuplicateProductCode => Conflict(new { error = "duplicate" }),
                _ => StatusCode(500, new { error = "unexpected" })
            });
    }

    [HttpPost("stock-line")]
    public async Task<IActionResult> CreateStockLine(
        [FromBody] CreateStockLineRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.IdProducto) || string.IsNullOrWhiteSpace(body.Location))
            return BadRequest(new { error = "Missing idProducto, location, or qty" });

        var location = InventoryItemResourceAssembler.ResolveLocation(body.Location);
        var command = new CreateStockLineCommand(
            new ProductCode(body.IdProducto),
            new InventoryLineCode(NextLineCode()),
            Math.Max(0, body.Qty),
            location,
            ParseDate(body.FechaIngreso) ?? DateOnly.FromDateTime(DateTime.UtcNow));

        var result = await commandService.Handle(command, ct);
        return result.Fold(
            line => Created(string.Empty, InventoryItemResourceAssembler.ToResource(line)),
            error => error switch
            {
                InventoryCommandError.ProductNotFound => NotFound(new { error = "product_not_found" }),
                _ => StatusCode(500, new { error = "unexpected" })
            });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateInventoryItemRequest body, CancellationToken ct)
    {
        if (body.Id != null && !string.Equals(body.Id, id, StringComparison.Ordinal))
            return BadRequest(new { error = "id mismatch" });

        var command = new UpdateInventoryItemCommand(
            new InventoryLineCode(id),
            body.Qty,
            body.Location is string loc ? InventoryItemResourceAssembler.ResolveLocation(loc) : null,
            body.Name is { } n && n is string ns ? ns : ExtractPlain(body.Name),
            body.Category is string cs ? cs : ExtractPlain(body.Category),
            body.Status != null ? InventoryItemResourceAssembler.UiStatusToDomain(body.Status) : null,
            body.TemperaturaMin,
            body.TemperaturaMax,
            body.Lote,
            ParseDate(body.FechaVencimiento));

        var result = await commandService.Handle(command, ct);
        return result.Fold(
            line => Ok(InventoryItemResourceAssembler.ToResource(line)),
            error => error == InventoryCommandError.LineNotFound
                ? NotFound(new { error = "Not found" })
                : StatusCode(500, new { error = "unexpected" }));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await commandService.Handle(
            new DeleteInventoryLineCommand(new InventoryLineCode(id)), ct);
        return result.Fold<IActionResult>(
            _ => NoContent(),
            error => error == InventoryCommandError.LineNotFound
                ? NotFound(new { error = "Not found" })
                : StatusCode(500, new { error = "unexpected" }));
    }

    private static string NextProductCode() => $"PROD-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    private static string NextLineCode() => $"INV-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, out var d) ? d : null;

    private static string? ExtractPlain(object? value)
    {
        if (value == null) return null;
        if (value is string s) return s;
        return null;
    }
}

public record CreateInventoryItemRequest(
    string? Name,
    string? Category,
    int? Qty,
    decimal? TempMin,
    decimal? TempMax,
    decimal? TemperaturaMin,
    decimal? TemperaturaMax,
    string? Location,
    string? Status,
    string? Lote,
    string? FechaVencimiento,
    string? FechaIngreso);

public record CreateStockLineRequest(
    string IdProducto,
    string Location,
    int Qty,
    string? FechaIngreso);

public record UpdateInventoryItemRequest(
    string? Id,
    int? Qty,
    string? Location,
    object? Name,
    object? Category,
    string? Status,
    decimal? TemperaturaMin,
    decimal? TemperaturaMax,
    string? Lote,
    string? FechaVencimiento);
