namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;

/// <summary>
/// Represents a delivery destination.
/// </summary>
public class LogisticsDestination
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Unique business code for the destination.</summary>
    public string DestinationCode { get; set; } = null!;

    /// <summary>URL-friendly identifier.</summary>
    public string Slug { get; set; } = null!;

    /// <summary>Multilingual name stored as JSON.</summary>
    public string NameJson { get; set; } = null!;
}