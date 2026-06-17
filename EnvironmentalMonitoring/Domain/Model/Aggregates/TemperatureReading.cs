using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Commands;
using SafeFlow.API.EnvironmentalMonitoring.Domain.Model.ValueObjects;

namespace SafeFlow.API.EnvironmentalMonitoring.Domain.Model.Aggregates;

public partial class TemperatureReading
{
    protected TemperatureReading()
    {
        ReadingCode = null!;
        ProductCode = null!;
        Source = null!;
    }

    public TemperatureReading(RecordTemperatureReadingCommand command)
    {
        ReadingCode = command.ReadingCode;
        ProductCode = command.ProductCode;
        Temperature = command.Temperature;
        Source = command.Source;
        RecordedAt = command.RecordedAt;
    }

    public int Id { get; private set; }
    public TemperatureReadingCode ReadingCode { get; private set; }
    public string ProductCode { get; private set; }
    public decimal Temperature { get; private set; }
    public string Source { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
}
