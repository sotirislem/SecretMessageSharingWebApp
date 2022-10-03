using Microsoft.IdentityModel.Tokens;
using SecretMessageSharingWebApp.Configuration;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecretMessageSharingWebApp.Services;

public sealed class JwtService : IJwtService
{
	private readonly IDateTimeProviderService _dateTimeProviderService;
	private readonly JwtConfigurationSettings _jwtConfigurationSettings;
	private readonly JwtSecurityTokenHandler _tokenHandler = new();

	public JwtService(JwtConfigurationSettings jwtConfigurationSettings, IDateTimeProviderService dateTimeProviderService)
	{
		_jwtConfigurationSettings = jwtConfigurationSettings;
		_dateTimeProviderService = dateTimeProviderService;
	}

	public string GenerateToken(List<Claim> claims, TimeSpan? expirationTimespan = null)
	{
		if (expirationTimespan is null)
		{
			expirationTimespan = TimeSpan.FromMinutes(Constants.JwtDefaultExpirationMinutes);
		}

		var expireDateTime = _dateTimeProviderService.UtcNow().Add((TimeSpan)expirationTimespan);

		var signingCredentials = new SigningCredentials(GetSigningSecurityKey(), SecurityAlgorithms.HmacSha256Signature);
		var jwtSecurityToken = new JwtSecurityToken(
			issuer: null,
			audience: null,
			claims: claims,
			notBefore: null,
			expires: expireDateTime,
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
		var jwtSigningKeyBytes = Encoding.UTF8.GetBytes(_jwtConfigurationSettings.SigningKey);
		var securityKey = new SymmetricSecurityKey(jwtSigningKeyBytes);
		return securityKey;
	}
}
