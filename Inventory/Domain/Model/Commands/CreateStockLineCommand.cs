using SafeFlow.API.Inventory.Domain.Model.ValueObjects;

namespace SafeFlow.API.Inventory.Domain.Model.Commands;

public record CreateStockLineCommand(
    ProductCode ProductCode,
    InventoryLineCode LineCode,
    int Quantity,
    string Location,
    DateOnly EntryDate);
