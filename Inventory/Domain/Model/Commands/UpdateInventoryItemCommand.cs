using SafeFlow.API.Inventory.Domain.Model.ValueObjects;

namespace SafeFlow.API.Inventory.Domain.Model.Commands;

public record UpdateInventoryItemCommand(
    InventoryLineCode LineCode,
    int? Quantity,
    string? Location,
    string? Name,
    string? Category,
    string? Status,
    decimal? TemperatureMin,
    decimal? TemperatureMax,
    string? Batch,
    DateOnly? ExpiryDate);
