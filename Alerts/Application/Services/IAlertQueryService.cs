using SafeFlow.API.Alerts.Domain.Model.Queries;

namespace SafeFlow.API.Alerts.Application.Services;

public interface IAlertQueryService
{
    Task<object> Handle(GetAlertsDashboardQuery query, CancellationToken ct = default);
    Task<object> ListAlertasAsync(CancellationToken ct = default);
}
