using Microsoft.IdentityModel.Tokens;
using SecretMessageSharingWebApp.Configuration;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecretMessageSharingWebApp.Services;

public sealed class JwtService(
	JwtConfigurationSettings jwtConfigurationSettings,
	ILogger<JwtService> logger,
	IDateTimeProviderService dateTimeProviderService) : IJwtService
{
	private readonly JwtSecurityTokenHandler _tokenHandler = new();

	public string GenerateToken(string messageId)
	{
		var utcNow = dateTimeProviderService.UtcNow();
		var expireDateTime = utcNow.AddMinutes(Constants.JwtTokenExpirationMinutes);

		var claims = new List<Claim>
		{
			new Claim(type: "messageId", value: messageId)
		};

		var signingCredentials = new SigningCredentials(GetSigningSecurityKey(), SecurityAlgorithms.HmacSha256Signature);
		var jwtSecurityToken = new JwtSecurityToken(
			issuer: Constants.AppName,
			audience: null,
			claims: claims,
			notBefore: utcNow,
			expires: expireDateTime,
			signingCredentials);

		try
		{
			var jwtToken = _tokenHandler.WriteToken(jwtSecurityToken);
			return jwtToken;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "JwtToken generation failed");

			throw new InvalidOperationException("JwtToken generation failed", ex);
		}
	}

	public bool ValidateToken(string? token, string messageId)
	{
		if (string.IsNullOrEmpty(token))
		{
			return false;
		}

		try
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = GetSigningSecurityKey(),
				ValidateIssuer = true,
				ValidIssuer = Constants.AppName,
				ValidateAudience = false,
				ClockSkew = TimeSpan.Zero
			};

			_tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

			JwtSecurityToken jwtToken = (JwtSecurityToken)securityToken;

			var jwtTokenClaimsAreValid = jwtToken.Claims.Any(claim => claim.Type == "messageId" && claim.Value == messageId);
			return jwtTokenClaimsAreValid;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "JwtToken validation failed");

			return false;
		}
	}

	private SecurityKey GetSigningSecurityKey()
	{
		var jwtSigningKeyBytes = Encoding.UTF8.GetBytes(jwtConfigurationSettings.SigningKey);
		var securityKey = new SymmetricSecurityKey(jwtSigningKeyBytes);
		return securityKey;
	}
}
