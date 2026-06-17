namespace SafeFlow.API.Alerts.Domain.Model.ValueObjects;

public sealed record AlertCode
{
    public AlertCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AlertCode cannot be empty.", nameof(value));
        Value = value.Trim();
    }

    public string Value { get; }
}
