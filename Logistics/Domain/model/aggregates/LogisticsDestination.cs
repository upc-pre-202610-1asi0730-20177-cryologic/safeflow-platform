namespace SafeFlow.API.Logistics.Domain.Model.Aggregates;


public class LogisticsDestination
{
    public int Id { get; set; }
    public string DestinationCode { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string NameJson { get; set; } = null!;
}