using Microsoft.IdentityModel.Tokens;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecretMessageSharingWebApp.Services
{
	public class JwtService : IJwtService
	{
		private readonly IConfiguration _configuration;
		private readonly JwtSecurityTokenHandler _tokenHandler = new();

		private readonly TimeSpan DefaultExpirationTimespan = TimeSpan.FromMinutes(1);

		public JwtService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string GenerateToken(List<Claim> claims, TimeSpan? expirationTimespan = null)
		{
			if (expirationTimespan is null)
			{
				expirationTimespan = DefaultExpirationTimespan;
			}
			
			var signingCredentials = new SigningCredentials(GetSigningSecurityKey(), SecurityAlgorithms.HmacSha256Signature);
			var jwtSecurityToken = new JwtSecurityToken(
				issuer: null,
				audience: null,
				claims: claims,
				notBefore: null,
				expires: DateTime.UtcNow.Add((TimeSpan)expirationTimespan),
				signingCredentials);

			var token = _tokenHandler.WriteToken(jwtSecurityToken);
			return token;
		}

		public JwtSecurityToken? ValidateToken(string? token)
		{
			if (string.IsNullOrEmpty(token))
			{
				return null;
			}

			try
			{
				var tokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = GetSigningSecurityKey(),
					ValidateIssuer = false,
					ValidateAudience = false,
					ClockSkew = TimeSpan.Zero
				};

				_tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

				var jwtToken = (JwtSecurityToken)validatedToken;
				return jwtToken;
			}
			catch
			{
				return null;
			}
		}

		private SecurityKey GetSigningSecurityKey()
		{
			var jwtSigningKey = _configuration["JwtSigningKey"];
			var jwtSigningKeyBytes = Encoding.UTF8.GetBytes(jwtSigningKey);
			var securityKey = new SymmetricSecurityKey(jwtSigningKeyBytes);
			return securityKey;
		}
	}
}
