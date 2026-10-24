using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class DateTimeProviderServiceTests
{
	private const string LocalTimeZoneId = "GTB Standard Time";

	private readonly DateTimeProviderService _sut;

	public DateTimeProviderServiceTests()
	{
		_sut = new DateTimeProviderService();
	}

	[Fact]
	public void UtcNow_ShouldReturnCurrentUtcTime()
	{
		// Arrange
		var before = DateTime.UtcNow;

		// Act
		var result = _sut.UtcNow();

		// Assert
		var after = DateTime.UtcNow;
		result.Kind.Should().Be(DateTimeKind.Utc);
		result.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
	}

	[Fact]
	public void LocalNow_ShouldReturnCurrentTimeInConfiguredTimeZone()
	{
		// Arrange
		var timeZone = TimeZoneInfo.FindSystemTimeZoneById(LocalTimeZoneId);
		var before = DateTime.UtcNow;

		// Act
		var result = _sut.LocalNow();

		// Assert
		var after = DateTime.UtcNow;
		var earliestExpected = TimeZoneInfo.ConvertTime(before, timeZone);
		var latestExpected = TimeZoneInfo.ConvertTime(after, timeZone);

		result.Should().BeOnOrAfter(earliestExpected).And.BeOnOrBefore(latestExpected);
	}
}
