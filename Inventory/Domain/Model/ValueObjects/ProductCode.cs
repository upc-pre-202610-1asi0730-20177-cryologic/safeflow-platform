namespace SafeFlow.API.Inventory.Domain.Model.ValueObjects;

public sealed record ProductCode
{
    private const int MaxLength = 64;

    public ProductCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ProductCode cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentException($"ProductCode cannot exceed {MaxLength} characters.", nameof(value));
        Value = value.Trim();
    }

    public string Value { get; }
    public override string ToString() => Value;
}
