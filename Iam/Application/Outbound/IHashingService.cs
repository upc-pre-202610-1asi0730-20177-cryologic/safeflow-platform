namespace SafeFlow.API.Iam.Application.Outbound;

public interface IHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
