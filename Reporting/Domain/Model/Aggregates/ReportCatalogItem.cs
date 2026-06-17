namespace SafeFlow.API.Reporting.Domain.Model.Aggregates;

public class ReportCatalogItem
{
    public int Id { get; set; }
    public string CatalogCode { get; set; } = null!;
    public string TitleJson { get; set; } = null!;
    public string Format { get; set; } = "pdf";
    public string? DescriptionJson { get; set; }
}
