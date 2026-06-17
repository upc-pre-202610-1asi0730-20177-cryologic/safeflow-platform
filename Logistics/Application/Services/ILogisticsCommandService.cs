using SafeFlow.API.Logistics.Application.Errors;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Application.Patterns;

namespace SafeFlow.API.Logistics.Application.Services;

/// <summary>
/// Defines the command operations service contract for handling logistics data persistence,
/// including destination configurations, transport personnel, and dispatch shipment tracking.
/// </summary>
public interface ILogisticsCommandService
{
    /// <summary>Creates a logistics destination and its associated route path synchronously.</summary>
    Task<Result<(LogisticsDestination Destino, LogisticsRoute Ruta), LogisticsCommandError>> CreateDestinoAsync(
        CreateDestinoDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing destination and propagates updates to its connected route.</summary>
    Task<Result<LogisticsDestination, LogisticsCommandError>> UpdateDestinoAsync(
        string destinationCode, UpdateDestinoDto dto, CancellationToken ct = default);

    /// <summary>Deletes a destination if referential integrity constraints allow.</summary>
    Task<Result<Unit, LogisticsCommandError>> DeleteDestinoAsync(string destinationCode, CancellationToken ct = default);

    /// <summary>Registers a new driver or operator under a carrier profile.</summary>
    Task<Result<LogisticsDriver, LogisticsCommandError>> CreateChoferAsync(
        CreateChoferDto dto, CancellationToken ct = default);

    /// <summary>Updates contact and identification details for tracking personnel.</summary>
    Task<Result<LogisticsDriver, LogisticsCommandError>> UpdateChoferAsync(
        string driverCode, UpdateChoferDto dto, CancellationToken ct = default);

    /// <summary>Removes a driver from the register if free of active dispatches.</summary>
    Task<Result<Unit, LogisticsCommandError>> DeleteChoferAsync(string driverCode, CancellationToken ct = default);

    /// <summary>Orchestrates and books a product shipment, deducting inventory line totals.</summary>
    Task<Result<LogisticsDispatch, LogisticsCommandError>> CreateShipmentAsync(
        CreateShipmentDto dto, CancellationToken ct = default);
}

/// <summary>Data contract for instantiating a new location destination entry.</summary>
public record CreateDestinoDto(string? Nombre, string? NombreEn, string? NombreEs, string? Codigo, string? Origen);

/// <summary>Data contract for applying delta updates to an established location destination entry.</summary>
public record UpdateDestinoDto(string? Nombre, string? NombreEn, string? NombreEs, string? Codigo, string? Origen);

/// <summary>Data contract for establishing a newly onboarded carrier personnel driver profile.</summary>
public record CreateChoferDto(
    string Codigo,
    string? Nombre,
    string? NombreEn,
    string? NombreEs,
    string? Licencia,
    string? Contacto,
    string? Rol,
    string? IdTransportista);

/// <summary>Data contract for mutating profile records of field operators or drivers.</summary>
public record UpdateChoferDto(
    string? Codigo,
    string? Nombre,
    string? NombreEn,
    string? NombreEs,
    string? Licencia,
    string? Contacto,
    string? Rol);

/// <summary>Data contract encompassing the volumetric and tracking parameters needed to book a dispatch.</summary>
public record CreateShipmentDto(
    string InventoryItemId,
    int Qty,
    string? OriginKey,
    string? DestinationKey,
    string? ChoferCodigo,
    string? OperarioCodigo,
    string Status);

/// <summary>Represents a terminal, value-absent execution outcome payload equivalent to void.</summary>
public readonly struct Unit;