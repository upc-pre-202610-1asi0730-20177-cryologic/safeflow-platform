using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Logistics.Application.Errors;
using SafeFlow.API.Logistics.Application.Services;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Logistics.Domain.Repositories;
using SafeFlow.API.Logistics.Interfaces.REST.Transform;
using SafeFlow.API.Shared.Application.Patterns;
using SafeFlow.API.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Logistics.Interfaces.REST;

/// <summary>
/// REST API controller for managing logistics shipments.
/// Coordinates creation, retrieval, and assembly of shipment data across Inventory and Logistics domains.
/// </summary>
/// <remarks>
/// Base route: <c>api/logistics/shipments</c>
/// 
/// Dependencies:
/// - <see cref="ILogisticsQueryService"/>: Read operations
/// - <see cref="ILogisticsCommandService"/>: Write operations
/// - <see cref="ILogisticsQueryRepository"/>: Domain queries (dispatches, carriers, drivers, routes)
/// - <see cref="IInventoryLineRepository"/>: Cross-domain inventory line queries
/// </remarks>
[ApiController]
[Route("api/logistics/shipments")]
[Authorize]
public class LogisticsShipmentsController(
    ILogisticsQueryService queryService,
    ILogisticsCommandService commandService,
    ILogisticsQueryRepository logisticsRepository,
    IInventoryLineRepository inventoryLineRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves all shipments.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 with assembled shipment collection.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await queryService.ListShipmentsAsync(ct));

    /// <summary>
    /// Creates a new shipment.
    /// </summary>
    /// <remarks>
    /// On success, rebuilds the complete shipment resource graph from repositories to ensure consistency.
    /// Returns the newly created shipment matched by dispatch code.
    /// </remarks>
    /// <param name="body">Shipment creation data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// HTTP 201 (Created) with the newly created shipment resource;
    /// HTTP 400 (Bad Request) if validation, inventory item, driver, destination, qty, or status is invalid;
    /// HTTP 404 (Not Found) if inventory item does not exist;
    /// HTTP 500 (Internal Server Error) on unexpected failure.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest body, CancellationToken ct)
    {
        var result = await commandService.CreateShipmentAsync(
            new CreateShipmentDto(
                body.InventoryItemId ?? "",
                body.Qty ?? 0,
                body.OriginKey,
                body.DestinationKey,
                body.ChoferCodigo,
                body.OperarioCodigo,
                body.Status ?? ""), ct);

        if (result is Result<LogisticsDispatch, LogisticsCommandError>.Failure failure)
            return MapError(failure.Error);

        var dispatch = ((Result<LogisticsDispatch, LogisticsCommandError>.Success)result).Value;
        var list = await BuildShipmentListAsync(ct);
        var legacy = list.FirstOrDefault(s => GetShipmentId(s) == dispatch.DispatchCode);
        return Created(string.Empty, legacy ?? new { id = dispatch.DispatchCode });
    }

    /// <summary>
    /// Assembles complete shipment resources by joining dispatches, carriers, drivers, routes, and inventory lines.
    /// </summary>
    /// <remarks>
    /// Performs multiple async queries across Logistics and Inventory domains and delegates assembly to <see cref="LogisticsShipmentAssembler"/>.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Assembled shipment object collection.</returns>
    private async Task<IReadOnlyList<object>> BuildShipmentListAsync(CancellationToken ct)
    {
        var dispatches = await logisticsRepository.ListDispatchesAsync(ct);
        var carriers = await logisticsRepository.ListCarriersAsync(ct);
        var drivers = await logisticsRepository.ListDriversAsync(ct);
        var routes = await logisticsRepository.ListRoutesAsync(ct);
        var lines = await inventoryLineRepository.ListWithProductsAsync(ct);
        return LogisticsShipmentAssembler.Build(dispatches, carriers, drivers, routes, lines);
    }

    /// <summary>
    /// Extracts the "id" property from a dynamic shipment object using reflection.
    /// </summary>
    /// <param name="shipment">The shipment object (typically an anonymous type).</param>
    /// <returns>The string value of the "id" property, or <c>null</c> if not found.</returns>
    private static string? GetShipmentId(object shipment) =>
        shipment.GetType().GetProperty("id")?.GetValue(shipment)?.ToString();

    /// <summary>
    /// Maps <see cref="LogisticsCommandError"/> to appropriate HTTP responses.
    /// </summary>
    /// <param name="error">The command error to map.</param>
    /// <returns>HTTP response corresponding to the error type.</returns>
    private IActionResult MapError(LogisticsCommandError error) => error switch
    {
        LogisticsCommandError.InvalidFields => BadRequest(new { error = "inventoryItemId required" }),
        LogisticsCommandError.InvalidStatus => BadRequest(new { error = "Invalid status" }),
        LogisticsCommandError.InventoryItemNotFound => NotFound(new { error = "inventory_item_not_found" }),
        LogisticsCommandError.InvalidDriver => BadRequest(new { error = "invalid_driver" }),
        LogisticsCommandError.InvalidDestinationOrDriver => BadRequest(new { error = "invalid_destination_or_driver" }),
        LogisticsCommandError.InvalidQty => BadRequest(new { error = "invalid_qty" }),
        _ => StatusCode(500, new { error = "unexpected" })
    };
}

/// <summary>
/// Data transfer object for shipment creation.
/// </summary>
/// <param name="InventoryItemId">ID of the inventory item to ship.</param>
/// <param name="Qty">Quantity to ship.</param>
/// <param name="OriginKey">Origin location identifier.</param>
/// <param name="DestinationKey">Destination location identifier.</param>
/// <param name="ChoferCodigo">Driver (chofer) code assigned to the shipment.</param>
/// <param name="OperarioCodigo">Operator code responsible for the shipment.</param>
/// <param name="Status">Initial shipment status.</param>
public record CreateShipmentRequest(
    string? InventoryItemId,
    int? Qty,
    string? OriginKey,
    string? DestinationKey,
    string? ChoferCodigo,
    string? OperarioCodigo,
    string? Status);
