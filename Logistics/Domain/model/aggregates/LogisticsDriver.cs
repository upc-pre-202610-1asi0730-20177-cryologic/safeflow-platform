namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;

public class LogisticsDriver
{
    public int Id { get; set; }
    public string DriverCode { get; set; } = null!;
    public string CarrierCode { get; set; } = null!;
    public string EmployeeCode { get; set; } = "";
    public string NameJson { get; set; } = null!;
    public string License { get; set; } = "";
    public string Contact { get; set; } = "";
    public string Role { get; set; } = "conductor";
}