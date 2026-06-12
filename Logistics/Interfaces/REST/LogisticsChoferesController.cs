using SafeFlow.API.Logistics.Application.Errors;
using SafeFlow.API.Logistics.Application.Services;
using SafeFlow.API.Logistics.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Logistics.Interfaces.REST;

/// <summary>
/// REST API controller for logistics drivers (choferes).
/// 
/// Handles all CRUD operations for driver management.
/// </summary>
/// <remarks>
/// Base route: <c>api/logistics/choferes</c>
///
/// Dependencies:
/// - <see cref="ILogisticsQueryService"/> for read operations.
/// - <see cref="ILogisticsCommandService"/> for write operations (create/update/delete).
/// </remarks>
[ApiController]
[Route("api/logistics/choferes")]
public class LogisticsChoferesController(
    ILogisticsQueryService queryService,
    ILogisticsCommandService commandService) : ControllerBase
{
    /// <summary>
    /// Returns all registered drivers.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP 200 with list of driver resources.</returns>
    /// <remarks>GET /api/logistics/choferes</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await queryService.ListChoferesAsync(ct));

    /// <summary>
    /// Creates a new driver.
    /// </summary>
    /// <param name="body">Driver data for creation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 Created with the new driver resource, 
    /// or 400/409 on validation or business rule violation.
    /// </returns>
    /// <remarks>POST /api/logistics/choferes</remarks>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ChoferWriteRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Codigo))
            return BadRequest(new { error = "invalid_chofer_fields" });

        var result = await commandService.CreateChoferAsync(
            new CreateChoferDto(
                body.Codigo,
                body.Nombre,
                body.NombreEn,
                body.NombreEs,
                body.Licencia,
                body.Contacto,
                body.Rol,
                body.IdTransportista), ct);

        return result.Fold<IActionResult>(
            c => Created(string.Empty, LogisticsResourceAssembler.ToChoferResource(c)),
            MapError);
    }

    /// <summary>
    /// Updates an existing driver.
    /// </summary>
    /// <param name="id">Driver identifier.</param>
    /// <param name="body">Updated driver data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 OK with updated driver, 
    /// or 400/404/409 on validation or business rule violation.
    /// </returns>
    /// <remarks>PUT /api/logistics/choferes/{id}</remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ChoferWriteRequest body, CancellationToken ct)
    {
        var result = await commandService.UpdateChoferAsync(
            id,
            new UpdateChoferDto(
                body.Codigo,
                body.Nombre,
                body.NombreEn,
                body.NombreEs,
                body.Licencia,
                body.Contacto,
                body.Rol), ct);

        return result.Fold<IActionResult>(
            c => Ok(LogisticsResourceAssembler.ToChoferResource(c)),
            MapError);
    }

    /// <summary>
    /// Deletes a driver.
    /// </summary>
    /// <param name="id">Driver identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 No Content on success, 
    /// or 404/409 if driver does not exist or is in use.
    /// </returns>
    /// <remarks>DELETE /api/logistics/choferes/{id}</remarks>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await commandService.DeleteChoferAsync(id, ct);
        return result.Fold<IActionResult>(
            _ => NoContent(),
            MapError);
    }

    /// <summary>
    /// Maps domain command errors to appropriate HTTP responses.
    /// </summary>
    /// <param name="error">Domain error to map.</param>
    private IActionResult MapError(LogisticsCommandError error) => error switch
    {
        LogisticsCommandError.InvalidFields     => BadRequest(new { error = "invalid_chofer_fields" }),
        LogisticsCommandError.CodigoExists      => Conflict(new { error = "codigo_exists" }),
        LogisticsCommandError.NotFound          => NotFound(new { error = "chofer_not_found" }),
        LogisticsCommandError.FleetNotConfigured => BadRequest(new { error = "fleet_not_configured" }),
        LogisticsCommandError.InUse             => Conflict(new { error = "chofer_in_use" }),
        _ => StatusCode(500, new { error = "unexpected" })
    };
}

/// <summary>
/// DTO for creating and updating drivers.
/// </summary>
/// <remarks>
/// All properties are nullable to allow flexible usage in both create and update scenarios.
/// <c>IdTransportista</c> is only used during creation (immutable afterwards).
/// </remarks>
public record ChoferWriteRequest(
    string? Codigo,
    string? Nombre,
    string? NombreEn,
    string? NombreEs,
    string? Licencia,
    string? Contacto,
    string? Rol,
    string? IdTransportista);