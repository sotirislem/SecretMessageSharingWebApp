using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SecretMessageSharingWebApp.Services.Interfaces
{
	public interface IJwtService
	{
		string GenerateToken(List<Claim> claims, TimeSpan? expirationTimespan = null);

		JwtSecurityToken? ValidateToken(string? token);
	}
}