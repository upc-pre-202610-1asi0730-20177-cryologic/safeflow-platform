using SafeFlow.API.Alerts.Application.Errors;
using SafeFlow.API.Alerts.Application.Services;
using SafeFlow.API.Alerts.Domain.Model.Aggregates;
using SafeFlow.API.Alerts.Domain.Model.Commands;
using SafeFlow.API.Alerts.Domain.Repositories;
using SafeFlow.API.Shared.Application.Patterns;
using SafeFlow.API.Shared.Domain.Repositories;

namespace SafeFlow.API.Alerts.Application.Internal.CommandServices;

public class AlertCommandService(
    IAlertRepository alertRepository,
    IUnitOfWork unitOfWork,
    ILogger<AlertCommandService> logger) : IAlertCommandService
{
    public async Task<Result<Alert, AlertCommandError>> Handle(
        CreateAlertCommand command, CancellationToken ct = default)
    {
        if (await alertRepository.FindByAlertCodeAsync(command.AlertCode, ct) != null)
            return new Result<Alert, AlertCommandError>.Failure(AlertCommandError.DuplicateCode);

        try
        {
            var alert = new Alert(command);
            await alertRepository.AddAsync(alert, ct);
            await unitOfWork.CompleteAsync(ct);
            return new Result<Alert, AlertCommandError>.Success(alert);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Create alert failed");
            return new Result<Alert, AlertCommandError>.Failure(AlertCommandError.UnexpectedError);
        }
    }

    public async Task<Result<Alert, AlertCommandError>> Handle(
        ResolveAlertCommand command, CancellationToken ct = default)
    {
        var alert = await alertRepository.FindByAlertCodeAsync(command.AlertCode, ct);
        if (alert == null)
            return new Result<Alert, AlertCommandError>.Failure(AlertCommandError.NotFound);

        alert.Resolve();
        alertRepository.Update(alert);
        await unitOfWork.CompleteAsync(ct);
        return new Result<Alert, AlertCommandError>.Success(alert);
    }
}
