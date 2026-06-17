using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.ValueObjects;

namespace SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Commands;

public record RecordTemperatureReadingCommand(
    TemperatureReadingCode ReadingCode,
    string ProductCode,
    decimal Temperature,
    string Source,
    DateTimeOffset RecordedAt);
