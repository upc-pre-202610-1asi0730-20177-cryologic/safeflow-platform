using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;
using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Inventory.Infrastructure.Persistence.EFC.Repositories;

public class ProductRepository(AppDbContext context)
    : BaseRepository<Product>(context), IProductRepository
{
    public async Task<Product?> FindByProductCodeAsync(ProductCode productCode, CancellationToken ct = default)
        => await Context.Products
            .FirstOrDefaultAsync(p => p.ProductCode == productCode, ct);

    public async Task<Product?> FindWithLinesAsync(int id, CancellationToken ct = default)
        => await Context.Products.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == id, ct);
}
