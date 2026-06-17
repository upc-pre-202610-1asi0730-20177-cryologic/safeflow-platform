namespace SafeFlow.API.Shared.Domain.Repositories;

public interface IBaseRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task<TEntity?> FindByIdAsync(int id, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<IEnumerable<TEntity>> ListAsync(CancellationToken ct = default);
}
