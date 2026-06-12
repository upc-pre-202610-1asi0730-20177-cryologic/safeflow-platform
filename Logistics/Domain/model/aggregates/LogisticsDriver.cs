namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;

/// <summary>
/// Represents a logistics driver (chofer).
/// </summary>
public class LogisticsDriver
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Unique driver code.</summary>
    public string DriverCode { get; set; } = null!;

    /// <summary>Carrier the driver belongs to.</summary>
    public string CarrierCode { get; set; } = null!;

    /// <summary>Internal employee code.</summary>
    public string EmployeeCode { get; set; } = "";

    /// <summary>Multilingual full name stored as JSON.</summary>
    public string NameJson { get; set; } = null!;

    /// <summary>Driver's license number.</summary>
    public string License { get; set; } = "";

    /// <summary>Contact information.</summary>
    public string Contact { get; set; } = "";

    /// <summary>Role within the fleet.</summary>
    public string Role { get; set; } = "conductor";
}