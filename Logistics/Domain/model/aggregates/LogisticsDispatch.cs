namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;

public class LogisticsDispatch
{
    public int Id { get; set; }
    public string DispatchCode { get; set; } = null!;
    public string CarrierCode { get; set; } = null!;
    public string? DriverCode { get; set; }
    public string? OperatorCode { get; set; }
    public string RouteCode { get; set; } = null!;
    public string InventoryLineCode { get; set; } = null!;
    public int Quantity { get; set; }
    public string Status { get; set; } = "pendiente";
    public string ThermalStatus { get; set; } = "seguro";
    public decimal? CurrentTemperature { get; set; }
    public DateTimeOffset? DepartureAt { get; set; }
    public DateTimeOffset? EstimatedArrivalAt { get; set; }
    public string PlacementMode { get; set; } = "ruta";
    public string? WarehouseLocationJson { get; set; }
    public string? TextsJson { get; set; }
}