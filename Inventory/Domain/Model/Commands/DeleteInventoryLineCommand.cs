using SafeFlow.API.Inventory.Domain.Model.ValueObjects;

namespace SafeFlow.API.Inventory.Domain.Model.Commands;

public record DeleteInventoryLineCommand(InventoryLineCode LineCode);
