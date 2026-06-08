using SafeFlow.API.Inventory.Application.Services;
using SafeFlow.API.Inventory.Domain.Model.Queries;
using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Inventory.Interfaces.REST.Resources;
using SafeFlow.API.Inventory.Interfaces.REST.Transform;

namespace SafeFlow.API.Inventory.Application.Internal.QueryServices;

public class InventoryQueryService(IInventoryLineRepository inventoryLineRepository) : IInventoryQueryService
{
    public async Task<IReadOnlyList<InventoryItemResource>> Handle(
        GetAllInventoryItemsQuery query, CancellationToken ct = default)
    {
        var lines = await inventoryLineRepository.ListWithProductsAsync(ct);
        return lines.Select(InventoryItemResourceAssembler.ToResource).ToList();
    }

    public async Task<InventoryItemResource?> Handle(
        GetInventoryItemByLineCodeQuery query, CancellationToken ct = default)
    {
        var line = await inventoryLineRepository.FindByLineCodeAsync(query.LineCode, ct);
        return line == null ? null : InventoryItemResourceAssembler.ToResource(line);
    }
}
