using SecretMessageSharingWebApp.Services;
using SecretMessageSharingWebApp.Services.Interfaces;
using System.Security.Claims;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public sealed class JwtServiceTests
{
	private readonly JwtService _sut;
	private readonly Fixture _fixture = new();

	private readonly IDateTimeProviderService _dateTimeProviderService = Substitute.For<IDateTimeProviderService>();

	public JwtServiceTests()
	{
		_sut = new JwtService(new() { SigningKey = _fixture.Create<string>() }, _dateTimeProviderService);

		_dateTimeProviderService.UtcNow().Returns(DateTime.UtcNow);
	}

	[Fact]
	public void ValidateToken_ShouldReturnJwtSecurityToken_WhenTokenIsValid()
	{
		// Arrange
		var guid = Guid.NewGuid().ToString();
		var claims = new List<Claim>()
		{
			new("id", guid)
		};
		var token = _sut.GenerateToken(claims);

		// Act
		var result = _sut.ValidateToken(token);

		// Assert
		result.Should().NotBeNull();
		result?.Claims.FirstOrDefault()?.Type.Should().Be("id");
		result?.Claims.FirstOrDefault()?.Value.Should().Be(guid);
	}

	[Fact]
	public void ValidateToken_ShouldReturnNull_WhenTokenIsExpired()
	{
		// Arrange
		var guid = Guid.NewGuid().ToString();
		var claims = new List<Claim>()
		{
			new("id", guid)
		};
		var token = _sut.GenerateToken(claims, TimeSpan.Zero);

		// Act
		var result = _sut.ValidateToken(token);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
	{
		// Arrange
		var token = _fixture.Create<string>();

		// Act
		var result = _sut.ValidateToken(token);

		// Assert
		result.Should().BeNull();
	}
}