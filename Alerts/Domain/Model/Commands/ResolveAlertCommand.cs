using SafeFlow.API.Alerts.Domain.Model.ValueObjects;

namespace SafeFlow.API.Alerts.Domain.Model.Commands;

public record ResolveAlertCommand(AlertCode AlertCode);
