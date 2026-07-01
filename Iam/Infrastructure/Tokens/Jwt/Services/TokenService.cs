using System.Security.Claims;
using System.Text;
using SafeFlow.API.Iam.Application.Outbound;
using SafeFlow.API.Iam.Domain.Model.Aggregates;
using SafeFlow.API.Iam.Infrastructure.Tokens.Jwt.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SafeFlow.API.Iam.Infrastructure.Tokens.Jwt.Services;

public class TokenService(IOptions<TokenSettings> tokenSettings) : ITokenService
{
    private readonly TokenSettings _settings = tokenSettings.Value;

    public string GenerateToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_settings.Secret);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            ]),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public async Task<int?> ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var key = Encoding.UTF8.GetBytes(_settings.Secret);
        try
        {
            var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            });

            if (!result.IsValid || result.SecurityToken is not JsonWebToken jwt)
                return null;

            var claim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}
