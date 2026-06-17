using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Aggregates;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.ValueObjects;
using SafeFlow.API.Shared.Domain.Repositories;

namespace SafeFlow.API.EnvironmentalMonitoring.Domain.Repositories;

public interface ITemperatureReadingRepository : IBaseRepository<TemperatureReading>
{
    Task<TemperatureReading?> FindByReadingCodeAsync(TemperatureReadingCode code, CancellationToken ct = default);
    Task<IReadOnlyList<TemperatureReading>> ListOrderedAsync(CancellationToken ct = default);
}
