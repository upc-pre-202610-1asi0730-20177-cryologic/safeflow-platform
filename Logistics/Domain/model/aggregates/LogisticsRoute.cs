namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;

/// <summary>
/// Represents a logistics route between origin and destination.
/// </summary>
public class LogisticsRoute
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Unique route code.</summary>
    public string RouteCode { get; set; } = null!;

    /// <summary>URL-friendly identifier.</summary>
    public string Slug { get; set; } = null!;

    /// <summary>Origin information stored as JSON.</summary>
    public string OriginJson { get; set; } = null!;

    /// <summary>Destination information stored as JSON.</summary>
    public string DestinationJson { get; set; } = null!;
}