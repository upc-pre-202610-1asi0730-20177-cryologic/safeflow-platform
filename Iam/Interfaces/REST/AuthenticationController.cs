using SafeFlow.API.Iam.Application.Errors;
using SafeFlow.API.Iam.Application.Services;
using SafeFlow.API.Iam.Domain.Model.Commands;
using SafeFlow.API.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using SafeFlow.API.Iam.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Iam.Interfaces.REST;

[ApiController]
[Route("api/authentication")]
[Authorize]
public class AuthenticationController(IUserCommandService userCommandService) : ControllerBase
{
    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource, CancellationToken ct)
    {
        var result = await userCommandService.Handle(
            new SignInCommand(resource.Username, resource.Password), ct);

        return result.Fold<IActionResult>(
            pair => Ok(new AuthenticatedUserResource(pair.User.Id, pair.User.Username, pair.Token)),
            error => error switch
            {
                IamCommandError.InvalidCredentials => BadRequest(new { error = "invalid_credentials" }),
                _ => StatusCode(500, new { error = "unexpected" })
            });
    }

    [HttpPost("sign-up")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource resource, CancellationToken ct)
    {
        var result = await userCommandService.Handle(
            new SignUpCommand(resource.Username, resource.Password), ct);

        return result.Fold<IActionResult>(
            _ => Ok(new { message = "user_created" }),
            error => error switch
            {
                IamCommandError.UsernameAlreadyTaken => Conflict(new { error = "username_taken" }),
                IamCommandError.DatabaseError => StatusCode(500, new { error = "database_error" }),
                _ => StatusCode(500, new { error = "unexpected" })
            });
    }
}
