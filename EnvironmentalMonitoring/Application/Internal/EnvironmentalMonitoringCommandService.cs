using SafeFlow.API.EnvironmentalMonitoring.Application.Services;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Aggregates;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Commands;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Repositories;
using SafeFlow.API.Shared.Application.Patterns;
using SafeFlow.API.Shared.Domain.Repositories;

namespace SafeFlow.API.EnvironmentalMonitoring.Application.Internal;

public class EnvironmentalMonitoringCommandService(
    ITemperatureReadingRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<EnvironmentalMonitoringCommandService> logger) : IEnvironmentalMonitoringCommandService
{
    public async Task<Result<TemperatureReading, string>> Handle(
        RecordTemperatureReadingCommand command, CancellationToken ct = default)
    {
        if (await repository.FindByReadingCodeAsync(command.ReadingCode, ct) != null)
            return new Result<TemperatureReading, string>.Failure("duplicate_code");

        try
        {
            var reading = new TemperatureReading(command);
            await repository.AddAsync(reading, ct);
            await unitOfWork.CompleteAsync(ct);
            return new Result<TemperatureReading, string>.Success(reading);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Record temperature reading failed");
            return new Result<TemperatureReading, string>.Failure("unexpected_error");
        }
    }
}
