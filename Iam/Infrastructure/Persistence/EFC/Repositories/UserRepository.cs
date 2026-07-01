using SafeFlow.API.Iam.Domain.Model.Aggregates;
using SafeFlow.API.Iam.Domain.Repositories;
using SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Iam.Infrastructure.Persistence.EFC.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> FindByIdAsync(int id, CancellationToken ct = default)
        => context.Set<User>().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default)
        => context.Set<User>().FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        => context.Set<User>().AnyAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await context.Set<User>().AddAsync(user, ct);

    public async Task<IEnumerable<User>> ListAsync(CancellationToken ct = default)
        => await context.Set<User>().ToListAsync(ct);
}
