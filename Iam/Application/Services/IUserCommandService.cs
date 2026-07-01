using SafeFlow.API.Iam.Domain.Model.Commands;
using SafeFlow.API.Iam.Domain.Model.Aggregates;
using SafeFlow.API.Iam.Application.Errors;
using SafeFlow.API.Shared.Application.Patterns;

namespace SafeFlow.API.Iam.Application.Services;

public interface IUserCommandService
{
    Task<Result<(User User, string Token), IamCommandError>> Handle(
        SignInCommand command, CancellationToken ct = default);

    Task<Result<bool, IamCommandError>> Handle(
        SignUpCommand command, CancellationToken ct = default);
}
