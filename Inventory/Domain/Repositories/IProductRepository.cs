using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;
using SafeFlow.API.Shared.Domain.Repositories;

namespace SafeFlow.API.Inventory.Domain.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> FindByProductCodeAsync(ProductCode productCode, CancellationToken ct = default);
    Task<Product?> FindWithLinesAsync(int id, CancellationToken ct = default);
}
