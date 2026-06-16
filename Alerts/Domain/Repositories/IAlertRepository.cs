using SafeFlow.API.Alerts.Domain.Model.Aggregates;
using SafeFlow.API.Alerts.Domain.Model.ValueObjects;
using SafeFlow.API.Shared.Domain.Repositories;

namespace SafeFlow.API.Alerts.Domain.Repositories;

public interface IAlertRepository : IBaseRepository<Alert>
{
    Task<Alert?> FindByAlertCodeAsync(AlertCode code, CancellationToken ct = default);
    Task<IReadOnlyList<Alert>> ListOrderedAsync(CancellationToken ct = default);
}
