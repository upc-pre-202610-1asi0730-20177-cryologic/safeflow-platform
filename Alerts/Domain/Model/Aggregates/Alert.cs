using SafeFlow.API.Alerts.Domain.Model.Commands;
using SafeFlow.API.Alerts.Domain.Model.ValueObjects;

namespace SafeFlow.API.Alerts.Domain.Model.Aggregates;

public partial class Alert
{
    protected Alert() { AlertCode = null!; AlertType = null!; Status = null!; Severity = null!; TitleJson = null!; }

    public Alert(CreateAlertCommand command)
    {
        AlertCode = command.AlertCode;
        AlertType = command.AlertType;
        Status = "activa";
        Severity = command.Severity;
        TitleJson = command.TitleJson;
        MessageJson = command.MessageJson;
        ProductCode = command.ProductCode;
        DispatchCode = command.DispatchCode;
        RecordedAt = command.RecordedAt;
    }

    public int Id { get; private set; }
    public AlertCode AlertCode { get; private set; }
    public string AlertType { get; private set; }
    public string Status { get; private set; }
    public string Severity { get; private set; }
    public string TitleJson { get; private set; }
    public string? MessageJson { get; private set; }
    public string? ProductCode { get; private set; }
    public string? DispatchCode { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }

    public void Resolve() => Status = "resuelta";
}
