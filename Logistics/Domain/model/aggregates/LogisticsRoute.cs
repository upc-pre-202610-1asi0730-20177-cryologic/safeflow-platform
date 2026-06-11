namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;
public class LogisticsRoute
{
    public int Id { get; set; }
    public int RouteCode { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string OriginJson { get; set; } = null!;
    public string DestinationJson { get; set; } = null!;
}