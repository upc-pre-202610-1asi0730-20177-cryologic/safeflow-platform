using SafeFlow.API.Iam.Application.Errors;
using SafeFlow.API.Iam.Application.Outbound;
using SafeFlow.API.Iam.Application.Services;
using SafeFlow.API.Iam.Domain.Model.Aggregates;
using SafeFlow.API.Iam.Domain.Model.Commands;
using SafeFlow.API.Iam.Domain.Repositories;
using SafeFlow.API.Shared.Application.Patterns;
using SafeFlow.API.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SafeFlow.API.Iam.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork) : IUserCommandService
{
    public async Task<Result<(User User, string Token), IamCommandError>> Handle(
        SignInCommand command, CancellationToken ct = default)
    {
        var user = await userRepository.FindByUsernameAsync(command.Username, ct);
        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            return new Result<(User, string), IamCommandError>.Failure(IamCommandError.InvalidCredentials);

        var token = tokenService.GenerateToken(user);
        return new Result<(User, string), IamCommandError>.Success((user, token));
    }

    public async Task<Result<bool, IamCommandError>> Handle(
        SignUpCommand command, CancellationToken ct = default)
    {
        if (await userRepository.ExistsByUsernameAsync(command.Username, ct))
            return new Result<bool, IamCommandError>.Failure(IamCommandError.UsernameAlreadyTaken);

        var user = new User(command.Username, hashingService.HashPassword(command.Password));
        try
        {
            await userRepository.AddAsync(user, ct);
            await unitOfWork.CompleteAsync(ct);
            return new Result<bool, IamCommandError>.Success(true);
        }
        catch (DbUpdateException)
        {
            return new Result<bool, IamCommandError>.Failure(IamCommandError.DatabaseError);
        }
    }
}
