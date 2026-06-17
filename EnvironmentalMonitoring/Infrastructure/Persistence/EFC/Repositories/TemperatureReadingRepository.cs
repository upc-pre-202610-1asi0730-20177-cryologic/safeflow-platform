using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Aggregates;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.ValueObjects;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.EnvironmentalMonitoring.Infrastructure.Persistence.EFC.Repositories;

public class TemperatureReadingRepository(AppDbContext context)
    : BaseRepository<TemperatureReading>(context), ITemperatureReadingRepository
{
    public async Task<TemperatureReading?> FindByReadingCodeAsync(
        TemperatureReadingCode code, CancellationToken ct = default)
        => await Context.TemperatureReadings.FirstOrDefaultAsync(r => r.ReadingCode == code, ct);

    public async Task<IReadOnlyList<TemperatureReading>> ListOrderedAsync(CancellationToken ct = default)
        => await Context.TemperatureReadings.AsNoTracking()
            .OrderByDescending(r => r.RecordedAt)
            .ToListAsync(ct);
}
