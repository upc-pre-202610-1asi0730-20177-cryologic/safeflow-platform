using SafeFlow.API.Inventory.Domain.Model.ValueObjects;

namespace SafeFlow.API.Inventory.Domain.Model.Queries;

public record GetInventoryItemByLineCodeQuery(InventoryLineCode LineCode);
