using SafeFlow.API.Inventory.Domain.Model.ValueObjects;

namespace SafeFlow.API.Inventory.Domain.Model.Commands;

public record CreateProductCommand(
    ProductCode ProductCode,
    string Name,
    string Category,
    decimal TemperatureMin,
    decimal TemperatureMax,
    DateOnly? ExpiryDate,
    string Batch,
    string Status,
    int InitialQuantity,
    string Location,
    DateOnly EntryDate,
    InventoryLineCode LineCode);
