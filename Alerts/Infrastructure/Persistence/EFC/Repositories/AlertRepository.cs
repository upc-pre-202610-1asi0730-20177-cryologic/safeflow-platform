using SafeFlow.API.Alerts.Domain.Model.Aggregates;
using SafeFlow.API.Alerts.Domain.Model.ValueObjects;
using SafeFlow.API.Alerts.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Alerts.Infrastructure.Persistence.EFC.Repositories;

public class AlertRepository(AppDbContext context) : BaseRepository<Alert>(context), IAlertRepository
{
    public async Task<Alert?> FindByAlertCodeAsync(AlertCode code, CancellationToken ct = default)
        => await Context.Alerts.FirstOrDefaultAsync(a => a.AlertCode == code, ct);

    public async Task<IReadOnlyList<Alert>> ListOrderedAsync(CancellationToken ct = default)
        => await Context.Alerts.AsNoTracking()
            .OrderByDescending(a => a.RecordedAt)
            .ToListAsync(ct);
}
