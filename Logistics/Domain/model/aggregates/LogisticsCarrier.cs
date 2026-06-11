namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;

public class LogisticsCarrier
{
    public int Id { get; set; }
    public string CarrierCode { get; set; } = null!;
    public string NameJson { get; set; } = null!;
    public string VehicleTypeJson { get; set; } = null!;
    public string Contact { get; set; } = "";
    public string FleetCode  { get; set; } = "";
}