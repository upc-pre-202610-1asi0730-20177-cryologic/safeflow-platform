using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Aggregates;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Commands;
using SafeFlow.API.Shared.Application.Patterns;

namespace SafeFlow.API.EnvironmentalMonitoring.Application.Services;

public interface IEnvironmentalMonitoringCommandService
{
    Task<Result<TemperatureReading, string>> Handle(
        RecordTemperatureReadingCommand command, CancellationToken ct = default);
}
