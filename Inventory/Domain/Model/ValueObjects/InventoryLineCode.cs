namespace SafeFlow.API.Inventory.Domain.Model.ValueObjects;

public sealed record InventoryLineCode
{
    private const int MaxLength = 64;

    public InventoryLineCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("InventoryLineCode cannot be empty.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentException($"InventoryLineCode cannot exceed {MaxLength} characters.", nameof(value));
        Value = value.Trim();
    }

    public string Value { get; }
    public override string ToString() => Value;
}
