namespace SafeFlow.API.Iam.Application.Outbound;

public interface ITokenService
{
    string GenerateToken(Domain.Model.Aggregates.User user);
    Task<int?> ValidateToken(string token);
}
