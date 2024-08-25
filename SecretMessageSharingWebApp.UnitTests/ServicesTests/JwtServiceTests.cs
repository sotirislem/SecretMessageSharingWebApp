using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SecretMessageSharingWebApp.Configuration;
using SecretMessageSharingWebApp.Services.Interfaces;
using SecretMessageSharingWebApp.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class JwtServiceTests
{
	private readonly IFixture _fixture;
	private readonly JwtConfigurationSettings _jwtConfigurationSettings;
	private readonly ILogger<JwtService> _logger;
	private readonly IDateTimeProviderService _dateTimeProviderService;

	private readonly JwtService _sut;

	public JwtServiceTests()
	{
		_fixture = new Fixture();
		_jwtConfigurationSettings = new JwtConfigurationSettings
		{
			SigningKey = _fixture.Create<string>()
		};
		_logger = Substitute.For<ILogger<JwtService>>();
		_dateTimeProviderService = Substitute.For<IDateTimeProviderService>();

		_sut = new JwtService(_jwtConfigurationSettings, _logger, _dateTimeProviderService);
	}

	[Fact]
	public void GenerateToken_ShouldReturnToken()
	{
		// Arrange
		var messageId = _fixture.Create<string>();

		_dateTimeProviderService.UtcNow().Returns(DateTime.UtcNow);

		// Act
		var result = _sut.GenerateToken(messageId);

		// Assert
		result.Should().NotBeNullOrEmpty();
		_logger.DidNotReceiveWithAnyArgs().LogError(default);
	}

	[Fact]
	public void ValidateToken_ShouldReturnFalseForInvalidToken()
	{
		// Arrange
		var invalidToken = _fixture.Create<string>();

		// Act
		var result = _sut.ValidateToken(invalidToken, _fixture.Create<string>());

		// Assert
		result.Should().BeFalse();
		_logger.ReceivedWithAnyArgs().LogError(default);
	}

	[Fact]
	public void ValidateToken_ShouldReturnTrueForValidToken()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var now = DateTime.UtcNow;
		var expireDateTime = now.AddMinutes(Constants.JwtTokenExpirationMinutes);
		var token = GenerateTestToken(messageId, now, expireDateTime);

		// Act
		var result = _sut.ValidateToken(token, messageId);

		// Assert
		result.Should().BeTrue();
		_logger.DidNotReceiveWithAnyArgs().LogError(default);
	}

	[Fact]
	public void ValidateToken_ShouldReturnFalseForExpiredToken()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var now = DateTime.UtcNow.AddMinutes(Constants.JwtTokenExpirationMinutes * 10 * -1);
		var expireDateTime = now.AddMinutes(Constants.JwtTokenExpirationMinutes);
		var token = GenerateTestToken(messageId, now, expireDateTime);

		// Act
		var result = _sut.ValidateToken(token, messageId);

		// Assert
		result.Should().BeFalse();
		_logger.ReceivedWithAnyArgs().LogError(default);
	}

	[Fact]
	public void ValidateToken_ShouldReturnFalseForTokenWithInvalidClaims()
	{
		// Arrange
		var messageId = _fixture.Create<string>();
		var now = DateTime.UtcNow;
		var expireDateTime = now.AddMinutes(Constants.JwtTokenExpirationMinutes);
		var token = GenerateTestToken(_fixture.Create<string>(), now, expireDateTime);

		// Act
		var result = _sut.ValidateToken(token, messageId);

		// Assert
		result.Should().BeFalse();
		_logger.DidNotReceiveWithAnyArgs().LogError(default);
	}

	private string GenerateTestToken(string messageId, DateTime utcNow, DateTime expires)
	{
		var claims = new List<Claim>
		{
			new Claim(type: "messageId", value: messageId)
		};

		var signingCredentials = new SigningCredentials(
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfigurationSettings.SigningKey)),
			SecurityAlgorithms.HmacSha256Signature
		);

		var jwtSecurityToken = new JwtSecurityToken(
			issuer: Constants.AppName,
			audience: null,
			claims: claims,
			notBefore: utcNow,
			expires: expires,
			signingCredentials: signingCredentials
		);

		return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
	}
}