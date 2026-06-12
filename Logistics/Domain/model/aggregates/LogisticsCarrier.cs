using SafeFlow.API.Logistics.Domain.Model.Aggregates;

namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;

/// <summary>
/// Represents a logistics carrier (transport company / fleet owner).
/// </summary>
public class LogisticsCarrier
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Unique business code for the carrier.</summary>
    public string CarrierCode { get; set; } = null!;

    /// <summary>Multilingual name stored as JSON.</summary>
    public string NameJson { get; set; } = null!;

    /// <summary>Supported vehicle types stored as JSON array.</summary>
    public string VehicleTypeJson { get; set; } = null!;

    /// <summary>Contact information (phone, email, etc.).</summary>
    public string Contact { get; set; } = "";

    /// <summary>Internal fleet identifier code.</summary>
    public string FleetCode { get; set; } = "";
}