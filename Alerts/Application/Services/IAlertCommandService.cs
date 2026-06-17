using SafeFlow.API.Alerts.Application.Errors;
using SafeFlow.API.Alerts.Domain.Model.Aggregates;
using SafeFlow.API.Alerts.Domain.Model.Commands;
using SafeFlow.API.Shared.Application.Patterns;

namespace SafeFlow.API.Alerts.Application.Services;

public interface IAlertCommandService
{
    Task<Result<Alert, AlertCommandError>> Handle(CreateAlertCommand command, CancellationToken ct = default);
    Task<Result<Alert, AlertCommandError>> Handle(ResolveAlertCommand command, CancellationToken ct = default);
}
