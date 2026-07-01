using SafeFlow.API.Iam.Application.Services;
using SafeFlow.API.Iam.Domain.Model.Aggregates;
using SafeFlow.API.Iam.Domain.Model.Queries;
using SafeFlow.API.Iam.Domain.Repositories;

namespace SafeFlow.API.Iam.Application.Internal.QueryServices;

public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    public Task<User?> Handle(GetUserByIdQuery query, CancellationToken ct = default)
        => userRepository.FindByIdAsync(query.Id, ct);

    public Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken ct = default)
        => userRepository.ListAsync(ct);

    public Task<User?> Handle(GetUserByUsernameQuery query, CancellationToken ct = default)
        => userRepository.FindByUsernameAsync(query.Username, ct);
}
