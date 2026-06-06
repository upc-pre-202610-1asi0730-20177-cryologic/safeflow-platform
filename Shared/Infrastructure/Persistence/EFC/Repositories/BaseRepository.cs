using SafeFlow.API.Shared.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Repositories;

public class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity>
    where TEntity : class
{
    protected readonly AppDbContext Context = context;

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await Context.Set<TEntity>().AddAsync(entity, ct);

    public async Task<TEntity?> FindByIdAsync(int id, CancellationToken ct = default)
        => await Context.Set<TEntity>().FindAsync([id], ct);

    public void Update(TEntity entity) => Context.Set<TEntity>().Update(entity);

    public void Remove(TEntity entity) => Context.Set<TEntity>().Remove(entity);

    public async Task<IEnumerable<TEntity>> ListAsync(CancellationToken ct = default)
        => await Context.Set<TEntity>().ToListAsync(ct);
}
