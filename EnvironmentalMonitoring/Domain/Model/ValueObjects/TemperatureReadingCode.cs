namespace SafeFlow.API.EnvironmentalMonitoring.Domain.Model.ValueObjects;

public sealed record TemperatureReadingCode
{
    public TemperatureReadingCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("TemperatureReadingCode cannot be empty.", nameof(value));
        Value = value.Trim();
    }

    public string Value { get; }
}
