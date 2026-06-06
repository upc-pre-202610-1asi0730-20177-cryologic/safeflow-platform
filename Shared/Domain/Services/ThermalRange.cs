namespace SafeFlow.API.Shared.Domain.Services;

public static class ThermalRange
{
    public static bool IsOutOfRange(decimal current, decimal min, decimal max)
        => current < min || current > max;

    public static string StatusFromReading(decimal current, decimal min, decimal max)
        => IsOutOfRange(current, min, max) ? "warning" : "safe";
}
