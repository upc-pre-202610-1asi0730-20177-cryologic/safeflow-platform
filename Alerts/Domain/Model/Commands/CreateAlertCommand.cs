using SafeFlow.API.Alerts.Domain.Model.ValueObjects;

namespace SafeFlow.API.Alerts.Domain.Model.Commands;

public record CreateAlertCommand(
    AlertCode AlertCode,
    string AlertType,
    string Severity,
    string TitleJson,
    string? MessageJson,
    string? ProductCode,
    string? DispatchCode,
    DateTimeOffset RecordedAt);
