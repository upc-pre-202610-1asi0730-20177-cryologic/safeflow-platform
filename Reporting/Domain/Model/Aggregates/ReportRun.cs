namespace SafeFlow.API.Reporting.Domain.Model.Aggregates;

public class ReportRun
{
    public int Id { get; set; }
    public string RunCode { get; set; } = null!;
    public string CatalogCode { get; set; } = null!;
    public string Format { get; set; } = "pdf";
    public string Status { get; set; } = "ready";
    public DateTimeOffset GeneratedAt { get; set; }
}
