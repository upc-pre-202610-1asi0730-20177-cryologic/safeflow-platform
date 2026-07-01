using SafeFlow.API.Iam.Domain.Model.Aggregates;
using SafeFlow.API.Iam.Domain.Model.Queries;

namespace SafeFlow.API.Iam.Application.Services;

public interface IUserQueryService
{
    Task<User?> Handle(GetUserByIdQuery query, CancellationToken ct = default);
    Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken ct = default);
    Task<User?> Handle(GetUserByUsernameQuery query, CancellationToken ct = default);
}
