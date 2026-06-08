using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;
using SafeFlow.API.Shared.Domain.Repositories;

namespace SafeFlow.API.Inventory.Domain.Repositories;

public interface IInventoryLineRepository : IBaseRepository<InventoryLine>
{
    Task<InventoryLine?> FindByLineCodeAsync(InventoryLineCode lineCode, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryLine>> ListWithProductsAsync(CancellationToken ct = default);
    Task<int> CountByProductIdAsync(int productId, CancellationToken ct = default);
}
