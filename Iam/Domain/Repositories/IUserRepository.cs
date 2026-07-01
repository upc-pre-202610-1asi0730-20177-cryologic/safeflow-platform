using SafeFlow.API.Iam.Domain.Model.Aggregates;

namespace SafeFlow.API.Iam.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id, CancellationToken ct = default);
    Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<IEnumerable<User>> ListAsync(CancellationToken ct = default);
}
