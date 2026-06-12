using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;
using SafeFlow.API.Logistics.Application.Errors;
using SafeFlow.API.Logistics.Application.Services;
using SafeFlow.API.Logistics.Domain.Model.Aggregates;
using SafeFlow.API.Shared.Application.Patterns;
using SafeFlow.API.Shared.Domain.Model;
using SafeFlow.API.Shared.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace SafeFlow.API.Logistics.Application.Internal.CommandServices;

/// <summary>
/// Service providing command operations for handling logistics data,
/// including destinations, routes, drivers, and dispatches/shipments.
/// </summary>
public class LogisticsCommandService(
    AppDbContext context,
    IUnitOfWork unitOfWork) : ILogisticsCommandService
{
    // Restricts acceptable UI status transitions to transit and pending.
    private static readonly HashSet<string> AllowedUiStatus = new(StringComparer.OrdinalIgnoreCase)
    {
        "transit", "pending"
    };

    /// <summary>
    /// Creates a new logistics destination and its associated route.
    /// </summary>
    /// <param name="dto">Data transfer object containing destination details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the created destination and route tuple, or a command error.</returns>
    public async Task<Result<(LogisticsDestination Destino, LogisticsRoute Ruta), LogisticsCommandError>>
        CreateDestinoAsync(CreateDestinoDto dto, CancellationToken ct = default)
    {
        var nombre = ResolveNombre(dto.Nombre, dto.NombreEn, dto.NombreEs);
        if (nombre.En == "—")
            return Fail<(LogisticsDestination, LogisticsRoute)>(LogisticsCommandError.InvalidFields);

        var destinos = await context.LogisticsDestinations.ToListAsync(ct);
        var rutas = await context.LogisticsRoutes.ToListAsync(ct);

        // Generate normalized unique identifier slug
        var slug = !string.IsNullOrWhiteSpace(dto.Codigo)
            ? LogisticsCodeHelper.NormalizeCodigo(dto.Codigo)
            : LogisticsCodeHelper.UniqueDestinoSlug(nombre.En, rutas, destinos);

        if (!LogisticsCodeHelper.IsValidCodigo(slug))
            return Fail<(LogisticsDestination, LogisticsRoute)>(LogisticsCommandError.InvalidFields);

        // Enforce global slug uniqueness across destinations and routes
        if (destinos.Any(d => d.Slug == slug) || rutas.Any(r => r.Slug == slug))
            return Fail<(LogisticsDestination, LogisticsRoute)>(LogisticsCommandError.CodigoExists);

        var origen = ResolveOrigen(dto.Origen);
        var destino = new LogisticsDestination
        {
            DestinationCode = LogisticsCodeHelper.NextDestinationCode(destinos),
            Slug = slug,
            NameJson = nombre.ToStorageJson()
        };

        var ruta = new LogisticsRoute
        {
            RouteCode = LogisticsCodeHelper.NextRouteCode(rutas),
            Slug = slug,
            OriginJson = origen.ToStorageJson(),
            DestinationJson = nombre.ToStorageJson()
        };

        context.LogisticsDestinations.Add(destino);
        context.LogisticsRoutes.Add(ruta);
        await unitOfWork.CompleteAsync(ct);

        return new Result<(LogisticsDestination, LogisticsRoute), LogisticsCommandError>.Success((destino, ruta));
    }

    /// <summary>
    /// Updates an existing logistics destination and synchronizes its corresponding route.
    /// </summary>
    /// <param name="destinationCode">Unique code identifier for the destination.</param>
    /// <param name="dto">Data transfer object containing update payloads.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the updated destination entity, or a command error.</returns>
    public async Task<Result<LogisticsDestination, LogisticsCommandError>> UpdateDestinoAsync(
        string destinationCode, UpdateDestinoDto dto, CancellationToken ct = default)
    {
        var destino = await context.LogisticsDestinations
            .FirstOrDefaultAsync(d => d.DestinationCode == destinationCode, ct);
        if (destino == null)
            return Fail<LogisticsDestination>(LogisticsCommandError.NotFound);

        var nombre = ResolveNombre(dto.Nombre, dto.NombreEn, dto.NombreEs);
        if (nombre.En == "—")
            return Fail<LogisticsDestination>(LogisticsCommandError.InvalidFields);

        var rutas = await context.LogisticsRoutes.ToListAsync(ct);
        var destinos = await context.LogisticsDestinations.ToListAsync(ct);
        var ruta = rutas.FirstOrDefault(r => r.Slug == destino.Slug);

        var nextSlug = !string.IsNullOrWhiteSpace(dto.Codigo)
            ? LogisticsCodeHelper.NormalizeCodigo(dto.Codigo)
            : destino.Slug;

        if (!LogisticsCodeHelper.IsValidCodigo(nextSlug))
            return Fail<LogisticsDestination>(LogisticsCommandError.InvalidFields);

        // Prevent slug collisions with other entities
        if (destinos.Any(d => d.Id != destino.Id && d.Slug == nextSlug)
            || rutas.Any(r => (ruta == null || r.Id != ruta.Id) && r.Slug == nextSlug))
            return Fail<LogisticsDestination>(LogisticsCommandError.CodigoExists);

        destino.Slug = nextSlug;
        destino.NameJson = nombre.ToStorageJson();

        // Propagate updates to matching route context
        if (ruta != null)
        {
            var origen = !string.IsNullOrWhiteSpace(dto.Origen)
                ? ResolveOrigen(dto.Origen)
                : LocalizedText.FromRaw(ruta.OriginJson);

            ruta.Slug = nextSlug;
            ruta.OriginJson = origen.ToStorageJson();
            ruta.DestinationJson = nombre.ToStorageJson();
        }

        await unitOfWork.CompleteAsync(ct);
        return new Result<LogisticsDestination, LogisticsCommandError>.Success(destino);
    }

    /// <summary>
    /// Deletes a destination and its corresponding route if it is not currently linked to any dispatches.
    /// </summary>
    /// <param name="destinationCode">Unique identifier for the destination.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An execution success indicator unit, or a command error.</returns>
    public async Task<Result<Unit, LogisticsCommandError>> DeleteDestinoAsync(
        string destinationCode, CancellationToken ct = default)
    {
        var destino = await context.LogisticsDestinations
            .FirstOrDefaultAsync(d => d.DestinationCode == destinationCode, ct);
        if (destino == null)
            return Fail<Unit>(LogisticsCommandError.NotFound);

        var ruta = await context.LogisticsRoutes.FirstOrDefaultAsync(r => r.Slug == destino.Slug, ct);
        if (ruta != null)
        {
            // Referential integrity check: Block deletion if route is actively used in dispatches
            var inUse = await context.LogisticsDispatches
                .AnyAsync(d => d.RouteCode == ruta.RouteCode, ct);
            if (inUse)
                return Fail<Unit>(LogisticsCommandError.InUse);

            context.LogisticsRoutes.Remove(ruta);
        }

        context.LogisticsDestinations.Remove(destino);
        await unitOfWork.CompleteAsync(ct);
        return new Result<Unit, LogisticsCommandError>.Success(default);
    }

    /// <summary>
    /// Registers a new driver or operator under a carrier profile.
    /// </summary>
    /// <param name="dto">Data transfer object containing personnel specs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The generated logistics driver profile entity, or a command error.</returns>
    public async Task<Result<LogisticsDriver, LogisticsCommandError>> CreateChoferAsync(
        CreateChoferDto dto, CancellationToken ct = default)
    {
        var codigo = LogisticsCodeHelper.NormalizeCodigo(dto.Codigo);
        var nombre = ResolveNombre(dto.Nombre, dto.NombreEn, dto.NombreEs);

        if (!LogisticsCodeHelper.IsValidCodigo(codigo) || nombre.En == "—")
            return Fail<LogisticsDriver>(LogisticsCommandError.InvalidFields);

        if (await context.LogisticsDrivers.AnyAsync(c => c.EmployeeCode == codigo, ct))
            return Fail<LogisticsDriver>(LogisticsCommandError.CodigoExists);

        var carrier = await EnsureDefaultCarrierAsync(ct);
        var carrierCode = !string.IsNullOrWhiteSpace(dto.IdTransportista)
            ? dto.IdTransportista.Trim()
            : carrier.CarrierCode;

        if (!await context.LogisticsCarriers.AnyAsync(c => c.CarrierCode == carrierCode, ct))
            return Fail<LogisticsDriver>(LogisticsCommandError.FleetNotConfigured);

        var drivers = await context.LogisticsDrivers.ToListAsync(ct);
        var driver = new LogisticsDriver
        {
            DriverCode = LogisticsCodeHelper.NextDriverCode(drivers),
            CarrierCode = carrierCode,
            EmployeeCode = codigo,
            NameJson = nombre.ToStorageJson(),
            License = dto.Licencia?.Trim() ?? "",
            Contact = dto.Contacto?.Trim() ?? "",
            Role = NormalizeRol(dto.Rol)
        };

        context.LogisticsDrivers.Add(driver);
        await unitOfWork.CompleteAsync(ct);
        return new Result<LogisticsDriver, LogisticsCommandError>.Success(driver);
    }

    /// <summary>
    /// Updates personnel data details for a driver or operator.
    /// </summary>
    /// <param name="driverCode">Internal logistics identifier code for the driver.</param>
    /// <param name="dto">Data transfer object hosting modification details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated logistics driver entity structure, or a command error.</returns>
    public async Task<Result<LogisticsDriver, LogisticsCommandError>> UpdateChoferAsync(
        string driverCode, UpdateChoferDto dto, CancellationToken ct = default)
    {
        var driver = await context.LogisticsDrivers
            .FirstOrDefaultAsync(c => c.DriverCode == driverCode, ct);
        if (driver == null)
            return Fail<LogisticsDriver>(LogisticsCommandError.NotFound);

        if (!string.IsNullOrWhiteSpace(dto.Codigo))
        {
            var codigo = LogisticsCodeHelper.NormalizeCodigo(dto.Codigo);
            if (!LogisticsCodeHelper.IsValidCodigo(codigo))
                return Fail<LogisticsDriver>(LogisticsCommandError.InvalidFields);

            if (await context.LogisticsDrivers.AnyAsync(c => c.Id != driver.Id && c.EmployeeCode == codigo, ct))
                return Fail<LogisticsDriver>(LogisticsCommandError.CodigoExists);

            driver.EmployeeCode = codigo;
        }

        var nombre = ResolveNombre(dto.Nombre, dto.NombreEn, dto.NombreEs);
        if (nombre.En != "—")
            driver.NameJson = nombre.ToStorageJson();

        if (dto.Licencia != null) driver.License = dto.Licencia.Trim();
        if (dto.Contacto != null) driver.Contact = dto.Contacto.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Rol)) driver.Role = NormalizeRol(dto.Rol);

        await unitOfWork.CompleteAsync(ct);
        return new Result<LogisticsDriver, LogisticsCommandError>.Success(driver);
    }

    /// <summary>
    /// Deletes a driver entity if they aren't assigned to any open dispatches as a driver or operator.
    /// </summary>
    /// <param name="driverCode">Internal tracking identifier for the driver profile.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An execution success indicator unit, or a command error.</returns>
    public async Task<Result<Unit, LogisticsCommandError>> DeleteChoferAsync(
        string driverCode, CancellationToken ct = default)
    {
        var driver = await context.LogisticsDrivers
            .FirstOrDefaultAsync(c => c.DriverCode == driverCode, ct);
        if (driver == null)
            return Fail<Unit>(LogisticsCommandError.NotFound);

        // Structural constraint: Verify personnel are free of active dispatches
        var inUse = await context.LogisticsDispatches.AnyAsync(
            d => d.DriverCode == driverCode || d.OperatorCode == driverCode, ct);
        if (inUse)
            return Fail<Unit>(LogisticsCommandError.InUse);

        context.LogisticsDrivers.Remove(driver);
        await unitOfWork.CompleteAsync(ct);
        return new Result<Unit, LogisticsCommandError>.Success(default);
    }

    /// <summary>
    /// Orchestrates and processes a shipment dispatch, validating inventory levels and assignment constraints.
    /// </summary>
    /// <param name="dto">Shipment specifications blueprint payload details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created logistics dispatch record transaction, or a command error.</returns>
    public async Task<Result<LogisticsDispatch, LogisticsCommandError>> CreateShipmentAsync(
        CreateShipmentDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.InventoryItemId))
            return Fail<LogisticsDispatch>(LogisticsCommandError.InvalidFields);

        var driverCode = LogisticsCodeHelper.NormalizeCodigo(dto.ChoferCodigo);
        var operatorCode = LogisticsCodeHelper.NormalizeCodigo(dto.OperarioCodigo);
        if (string.IsNullOrEmpty(driverCode) && string.IsNullOrEmpty(operatorCode))
            return Fail<LogisticsDispatch>(LogisticsCommandError.InvalidFields);

        if (!AllowedUiStatus.Contains(dto.Status))
            return Fail<LogisticsDispatch>(LogisticsCommandError.InvalidStatus);

        var line = await context.InventoryLines
            .Include(l => l.Product)
            .FirstOrDefaultAsync(l => l.LineCode == new InventoryLineCode(dto.InventoryItemId.Trim()), ct);
        if (line == null)
            return Fail<LogisticsDispatch>(LogisticsCommandError.InventoryItemNotFound);

        // Personnel Assignment Assertions
        var drivers = await context.LogisticsDrivers.ToListAsync(ct);
        LogisticsDriver? chDriver = null;
        LogisticsDriver? chOp = null;

        if (!string.IsNullOrEmpty(driverCode))
        {
            chDriver = drivers.FirstOrDefault(c => c.EmployeeCode == driverCode);
            if (chDriver == null || IsOperario(chDriver))
                return Fail<LogisticsDispatch>(LogisticsCommandError.InvalidDriver);
        }

        if (!string.IsNullOrEmpty(operatorCode))
        {
            chOp = drivers.FirstOrDefault(c => c.EmployeeCode == operatorCode);
            if (chOp == null || !IsOperario(chOp))
                return Fail<LogisticsDispatch>(LogisticsCommandError.InvalidDriver);
        }

        // Fleet assignment resolution
        var carrierCode = chDriver?.CarrierCode ?? chOp?.CarrierCode;
        if (string.IsNullOrEmpty(carrierCode) || !await context.LogisticsCarriers.AnyAsync(c => c.CarrierCode == carrierCode, ct))
        {
            var carrier = await EnsureDefaultCarrierAsync(ct);
            carrierCode = carrier.CarrierCode;
        }

        // Volumetric validation checks
        var maxQty = Math.Max(line.Quantity, 1);
        if (dto.Qty < 1 || dto.Qty > maxQty)
            return Fail<LogisticsDispatch>(LogisticsCommandError.InvalidQty);

        var domStatus = dto.Status.Equals("transit", StringComparison.OrdinalIgnoreCase) ? "en_transito" : "pendiente";
        var thermal = line.Product.Status == "en_riesgo" ? "en_riesgo" : "estable";
        decimal? temp = null;

        // Cold-chain climate control rules matrix application 
        if (domStatus == "en_transito")
        {
            temp = thermal == "en_riesgo"
                ? 8.6m
                : line.Product.TemperatureMax <= 0 || line.Product.TemperatureMin < -10
                    ? -18.5m
                    : 2.1m;
        }

        var now = DateTimeOffset.UtcNow;
        var eta = now.AddHours(36); // Fixed window ETA allocation
        var destKey = LogisticsCodeHelper.NormalizeCodigo(dto.DestinationKey);
        var originKey = LogisticsCodeHelper.NormalizeCodigo(dto.OriginKey);

        var dispatches = await context.LogisticsDispatches.ToListAsync(ct);
        var dispatch = new LogisticsDispatch
        {
            DispatchCode = LogisticsCodeHelper.NextDispatchCode(dispatches),
            CarrierCode = carrierCode!,
            Quantity = dto.Qty,
            Status = domStatus,
            ThermalStatus = thermal,
            CurrentTemperature = temp,
            DepartureAt = now,
            EstimatedArrivalAt = eta,
            InventoryLineCode = line.LineCode.Value
        };

        if (chDriver != null) dispatch.DriverCode = chDriver.DriverCode;
        if (chOp != null) dispatch.OperatorCode = chOp.DriverCode;

        // Route Strategy Resolution vs Warehouse Internal Transfers 
        if (string.IsNullOrEmpty(destKey))
        {
            var wh = await ResolvePlaceAsync(originKey, ct);
            dispatch.PlacementMode = "almacen";
            dispatch.RouteCode = "WH";
            dispatch.WarehouseLocationJson = wh.ToStorageJson();
            dispatch.TextsJson = JsonSerializer.Serialize(new
            {
                originPlace = wh.ToApiObject(),
                destPlace = wh.ToApiObject()
            });
        }
        else
        {
            var ruta = await context.LogisticsRoutes.FirstOrDefaultAsync(r => r.Slug == destKey, ct);
            if (ruta == null)
                return Fail<LogisticsDispatch>(LogisticsCommandError.InvalidDestinationOrDriver);

            var originPlace = await ResolvePlaceAsync(string.IsNullOrEmpty(originKey) ? "main_warehouse" : originKey, ct);
            var destPlace = LocalizedText.FromRaw(ruta.DestinationJson);
            dispatch.PlacementMode = "ruta";
            dispatch.RouteCode = ruta.RouteCode;
            dispatch.TextsJson = JsonSerializer.Serialize(new
            {
                originPlace = originPlace.ToApiObject(),
                destPlace = destPlace.ToApiObject()
            });
        }

        // Deduct inventory quantities through domain commands
        line.ApplyUpdate(new Inventory.Domain.Model.Commands.UpdateInventoryItemCommand(
            line.LineCode,
            Quantity: Math.Max(0, line.Quantity - dto.Qty),
            Location: null, Name: null, Category: null, Status: null,
            TemperatureMin: null, TemperatureMax: null, Batch: null, ExpiryDate: null));

        if (domStatus == "en_transito")
            line.Product.ApplyUpdate(new Inventory.Domain.Model.Commands.UpdateInventoryItemCommand(
                line.LineCode,
                null, null, null, null, "en_transito", null, null, null, null));

        context.LogisticsDispatches.Add(dispatch);
        await unitOfWork.CompleteAsync(ct);
        return new Result<LogisticsDispatch, LogisticsCommandError>.Success(dispatch);
    }

    // Ensures at least one fallback corporate logistic carrier setup exists.
    private async Task<LogisticsCarrier> EnsureDefaultCarrierAsync(CancellationToken ct)
    {
        var existing = await context.LogisticsCarriers.FirstOrDefaultAsync(ct);
        if (existing != null)
            return existing;

        var carrier = new LogisticsCarrier
        {
            CarrierCode = "T1",
            NameJson = new LocalizedText("SafeFlow Logistics", "SafeFlow Logística").ToStorageJson(),
            VehicleTypeJson = new LocalizedText("Refrigerated truck", "Camión refrigerado").ToStorageJson(),
            Contact = "",
            FleetCode = "sf_fleet"
        };

        context.LogisticsCarriers.Add(carrier);
        await unitOfWork.CompleteAsync(ct);
        return carrier;
    }

    // Resolves a localized place description matching against destinations, routes, or default fallbacks.
    private async Task<LocalizedText> ResolvePlaceAsync(string slug, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(slug) || slug == "main_warehouse")
            return new LocalizedText("Main warehouse", "Almacén principal");

        var dest = await context.LogisticsDestinations.FirstOrDefaultAsync(d => d.Slug == slug, ct);
        if (dest != null)
            return LocalizedText.FromRaw(dest.NameJson);

        var ruta = await context.LogisticsRoutes.FirstOrDefaultAsync(r => r.Slug == slug, ct);
        if (ruta != null)
            return LocalizedText.FromRaw(ruta.DestinationJson);

        return new LocalizedText(slug, slug);
    }

    // Cascades name lookup properties to determine accurate fallback labels.
    private static LocalizedText ResolveNombre(string? nombre, string? nombreEn, string? nombreEs)
    {
        var raw = FirstTrimmed(nombre, nombreEn, nombreEs);
        if (string.IsNullOrWhiteSpace(raw))
            return new LocalizedText("—", "—");

        return new LocalizedText(raw, raw);
    }

    // Parses text strings to determine geographic origins.
    private static LocalizedText ResolveOrigen(string? origen)
    {
        if (!string.IsNullOrWhiteSpace(origen))
        {
            var t = origen.Trim();
            return new LocalizedText(t, t);
        }
        return new LocalizedText("Main warehouse", "Almacén principal");
    }

    // Standardizes free text workforce categorization into valid system enums.
    private static string NormalizeRol(string? rol)
    {
        var r = (rol ?? "conductor").Trim().ToLowerInvariant();
        return r is "operario" or "operador" ? "operario" : "conductor";
    }

    // Evaluation to verify if a worker possesses structural operator access privileges.
    private static bool IsOperario(LogisticsDriver driver)
    {
        var r = driver.Role.Trim().ToLowerInvariant();
        return r is "operario" or "operador";
    }

    // Helper returning the first non-nullable trimmed argument string values array item.
    private static string? FirstTrimmed(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
        }
        return null;
    }

    // Instantiates a structured command failure pattern result wrapper.
    private static Result<T, LogisticsCommandError> Fail<T>(LogisticsCommandError error) =>
        new Result<T, LogisticsCommandError>.Failure(error);
}