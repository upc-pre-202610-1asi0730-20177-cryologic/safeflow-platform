using SafeFlow.API.Inventory.Domain.Model.Queries;
using SafeFlow.API.Inventory.Interfaces.REST.Resources;

namespace SafeFlow.API.Inventory.Application.Services;

public interface IInventoryQueryService
{
    Task<IReadOnlyList<InventoryItemResource>> Handle(
        GetAllInventoryItemsQuery query, CancellationToken ct = default);

    Task<InventoryItemResource?> Handle(
        GetInventoryItemByLineCodeQuery query, CancellationToken ct = default);
}
