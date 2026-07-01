using SafeFlow.API.Iam.Application.Outbound;
using BCryptNet = BCrypt.Net.BCrypt;

namespace SafeFlow.API.Iam.Infrastructure.Hashing.Services;

public class HashingService : IHashingService
{
    public string HashPassword(string password) => BCryptNet.HashPassword(password);

    public bool VerifyPassword(string password, string passwordHash)
        => BCryptNet.Verify(password, passwordHash);
}
