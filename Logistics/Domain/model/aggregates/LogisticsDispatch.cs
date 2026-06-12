namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;

/// <summary>
/// Represents a single logistics dispatch / shipment.
/// Core aggregate for tracking goods movement.
/// </summary>
public class LogisticsDispatch
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Unique dispatch identifier.</summary>
    public string DispatchCode { get; set; } = null!;

    /// <summary>Carrier performing the dispatch.</summary>
    public string CarrierCode { get; set; } = null!;

    /// <summary>Assigned driver.</summary>
    public string? DriverCode { get; set; }

    /// <summary>Operator / dispatcher code.</summary>
    public string? OperatorCode { get; set; }

    /// <summary>Route used for this dispatch.</summary>
    public string RouteCode { get; set; } = null!;

    /// <summary>Inventory line being transported.</summary>
    public string InventoryLineCode { get; set; } = null!;

    /// <summary>Quantity of items/units.</summary>
    public int Quantity { get; set; }

    /// <summary>Current dispatch status (e.g. pendiente, en_ruta, entregado).</summary>
    public string Status { get; set; } = "pendiente";

    /// <summary>Thermal condition status for sensitive cargo.</summary>
    public string ThermalStatus { get; set; } = "seguro";

    /// <summary>Current recorded temperature (for cold chain monitoring).</summary>
    public decimal? CurrentTemperature { get; set; }

    /// <summary>Actual departure timestamp.</summary>
    public DateTimeOffset? DepartureAt { get; set; }

    /// <summary>Estimated time of arrival.</summary>
    public DateTimeOffset? EstimatedArrivalAt { get; set; }

    /// <summary>Placement mode (ruta, directo, etc.).</summary>
    public string PlacementMode { get; set; } = "ruta";

    /// <summary>Warehouse location details in JSON.</summary>
    public string? WarehouseLocationJson { get; set; }

    /// <summary>Additional multilingual texts / notes stored as JSON.</summary>
    public string? TextsJson { get; set; }
}