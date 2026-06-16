using SafeFlow.API.Logistics.Application.Errors;
using SafeFlow.API.Logistics.Application.Services;
using SafeFlow.API.Logistics.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Logistics.Interfaces.REST;

/// <summary>
/// REST API controller for managing logistics destinations (destinos).
/// Provides endpoints for CRUD operations on destination records.
/// </summary>
/// <remarks>
/// Base route: <c>api/logistics/destinos</c>
/// 
/// Dependencies:
/// - <see cref="ILogisticsQueryService"/>: Read operations
/// - <see cref="ILogisticsCommandService"/>: Write operations
/// </remarks>
[ApiController]
[Route("api/logistics/destinos")]
public class LogisticsDestinosController(
    ILogisticsQueryService queryService,
    ILogisticsCommandService commandService) : ControllerBase
{
    /// <summary>
    /// Retrieves all destinations.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 with destination collection.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await queryService.ListDestinosAsync(ct));

    /// <summary>
    /// Creates a new destination.
    /// </summary>
    /// <param name="body">Destination data from request body.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// HTTP 201 (Created) with the created resource;
    /// HTTP 400 (Bad Request) if validation fails;
    /// HTTP 409 (Conflict) if code already exists;
    /// HTTP 500 (Internal Server Error) on unexpected failure.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DestinoWriteRequest body, CancellationToken ct)
    {
        var result = await commandService.CreateDestinoAsync(
            new CreateDestinoDto(body.Nombre, body.NombreEn, body.NombreEs, body.Codigo, body.Origen), ct);
        return result.Fold<IActionResult>(
            pair => Created(string.Empty, LogisticsResourceAssembler.ToDestinoResource(pair.Destino)),
            MapError);
    }

    /// <summary>
    /// Updates an existing destination.
    /// </summary>
    /// <param name="id">Destination identifier.</param>
    /// <param name="body">Updated destination data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// HTTP 200 (OK) with updated resource;
    /// HTTP 404 (Not Found) if destination does not exist;
    /// HTTP 409 (Conflict) if code is already in use;
    /// HTTP 400 (Bad Request) if validation fails;
    /// HTTP 500 (Internal Server Error) on unexpected failure.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] DestinoWriteRequest body, CancellationToken ct)
    {
        var result = await commandService.UpdateDestinoAsync(
            id, new UpdateDestinoDto(body.Nombre, body.NombreEn, body.NombreEs, body.Codigo, body.Origen), ct);
        return result.Fold<IActionResult>(
            d => Ok(LogisticsResourceAssembler.ToDestinoResource(d)),
            MapError);
    }

    /// <summary>
    /// Deletes a destination.
    /// </summary>
    /// <param name="id">Destination identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// HTTP 204 (No Content) if deletion succeeds;
    /// HTTP 404 (Not Found) if destination does not exist;
    /// HTTP 409 (Conflict) if destination is in use;
    /// HTTP 500 (Internal Server Error) on unexpected failure.
    /// </returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await commandService.DeleteDestinoAsync(id, ct);
        return result.Fold<IActionResult>(
            _ => NoContent(),
            MapError);
    }

    /// <summary>
    /// Maps <see cref="LogisticsCommandError"/> to appropriate HTTP responses.
    /// </summary>
    /// <param name="error">The command error to map.</param>
    /// <returns>HTTP response corresponding to the error type.</returns>
    private IActionResult MapError(LogisticsCommandError error) => error switch
    {
        LogisticsCommandError.InvalidFields => BadRequest(new { error = "invalid_destino_fields" }),
        LogisticsCommandError.CodigoExists => Conflict(new { error = "codigo_exists" }),
        LogisticsCommandError.NotFound => NotFound(new { error = "destino_not_found" }),
        LogisticsCommandError.InUse => Conflict(new { error = "destino_in_use" }),
        _ => StatusCode(500, new { error = "unexpected" })
    };
}

/// <summary>
/// Data transfer object for destination write operations (create/update).
/// </summary>
/// <param name="Nombre">Destination name in default language.</param>
/// <param name="NombreEn">Destination name in English.</param>
/// <param name="NombreEs">Destination name in Spanish.</param>
/// <param name="Codigo">Unique destination code.</param>
/// <param name="Origen">Origin or source information for the destination.</param>
public record DestinoWriteRequest(
    string? Nombre,
    string? NombreEn,
    string? NombreEs,
    string? Codigo,
    string? Origen);
