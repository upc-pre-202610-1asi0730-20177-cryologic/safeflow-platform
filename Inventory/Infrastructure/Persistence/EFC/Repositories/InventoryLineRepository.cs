using SafeFlow.API.Inventory.Domain.Model.Aggregates;
using SafeFlow.API.Inventory.Domain.Model.ValueObjects;
using SafeFlow.API.Inventory.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Inventory.Infrastructure.Persistence.EFC.Repositories;

public class InventoryLineRepository(AppDbContext context)
    : BaseRepository<InventoryLine>(context), IInventoryLineRepository
{
    public async Task<InventoryLine?> FindByLineCodeAsync(
        InventoryLineCode lineCode, CancellationToken ct = default)
        => await Context.InventoryLines
            .Include(l => l.Product)
            .FirstOrDefaultAsync(l => l.LineCode == lineCode, ct);

    public async Task<IReadOnlyList<InventoryLine>> ListWithProductsAsync(CancellationToken ct = default)
    {
        var lines = await Context.InventoryLines
            .Include(l => l.Product)
            .ToListAsync(ct);
        return lines.OrderBy(l => l.LineCode.Value).ToList();
    }

    public async Task<int> CountByProductIdAsync(int productId, CancellationToken ct = default)
        => await Context.InventoryLines.CountAsync(l => l.ProductId == productId, ct);
}
