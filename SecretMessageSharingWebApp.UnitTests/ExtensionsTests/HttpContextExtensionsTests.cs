using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Middlewares;

namespace SecretMessageSharingWebApp.UnitTests.ExtensionsTests;

public class HttpContextExtensionsTests
{
	[Theory]
	[InlineData("::1")]
	[InlineData("127.0.0.1")]
	public void GetClientIP_ShouldReturnLocalhostForLoopbackRequestsInDevelopment(string loopbackAddress)
	{
		// Arrange
		var context = CreateHttpContext(IPAddress.Parse(loopbackAddress), Environments.Development);

		// Act
		var result = context.GetClientIP();

		// Assert
		result.Should().Be("localhost");
	}

	[Fact]
	public void GetClientIP_ShouldKeepLoopbackAddressOutsideDevelopment()
	{
		// Arrange
		var context = CreateHttpContext(IPAddress.IPv6Loopback, Environments.Production);

		// Act
		var result = context.GetClientIP();

		// Assert
		result.Should().Be("::1");
	}

	[Fact]
	public void GetClientIP_ShouldReturnRemoteAddressForNonLoopbackRequests()
	{
		// Arrange
		var context = CreateHttpContext(IPAddress.Parse("203.0.113.10"), Environments.Development);

		// Act
		var result = context.GetClientIP();

		// Assert
		result.Should().Be("203.0.113.10");
	}

	[Fact]
	public void GetClientIP_ShouldReturnUnknownWhenRemoteAddressIsMissing()
	{
		// Arrange
		var context = new DefaultHttpContext();

		// Act
		var result = context.GetClientIP();

		// Assert
		result.Should().Be("Unknown");
	}

	[Fact]
	public void GetClientInfo_ShouldIncludeTheAndroidDeviceModelFromClientHints()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Headers.UserAgent =
			"Mozilla/5.0 (Linux; Android 13; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36";
		context.Request.Headers["Sec-CH-UA-Model"] = "\"Pixel 7\"";

		// Act
		var result = context.GetClientInfo();

		// Assert
		result.Should().Contain("Pixel 7");
		result.Should().Contain("Mobile");
	}

	[Fact]
	public void GetClientInfo_ShouldPreferTheDeviceModelSentByTheSpa()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Headers.UserAgent =
			"Mozilla/5.0 (Linux; Android 13; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36";
		context.Request.Headers["X-Client-Device-Model"] = "Pixel 7 Pro";
		context.Request.Headers["Sec-CH-UA-Model"] = "\"Pixel 7\"";

		// Act
		var result = context.GetClientInfo();

		// Assert
		result.Should().Contain("Pixel 7 Pro");
		result.Should().NotContain("\"Pixel 7\"");
	}

	[Fact]
	public void GetClientInfo_ShouldIgnoreAnEmptyDeviceModel()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Headers.UserAgent =
			"Mozilla/5.0 (Linux; Android 13; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36";
		// Desktop Chromium sends an empty model value.
		context.Request.Headers["Sec-CH-UA-Model"] = "\"\"";

		// Act
		var result = context.GetClientInfo();

		// Assert
		result.Should().NotContain("()");
		result.Should().NotContain("\"\"");
		result.Should().NotContain("Pixel");
	}

	[Fact]
	public void GetClientInfo_ShouldRejectUntrustedDeviceModelValues()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Headers.UserAgent =
			"Mozilla/5.0 (Linux; Android 13; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36";
		context.Request.Headers["Sec-CH-UA-Model"] = "\"<script>alert(1)</script>\"";

		// Act
		var result = context.GetClientInfo();

		// Assert
		result.Should().NotContain("<script>");
		result.Should().NotContain("script");
	}

	[Fact]
	public void GetClientInfo_ShouldReturnNullWithoutAUserAgent()
	{
		// Act
		var result = new DefaultHttpContext().GetClientInfo();

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public void GetHttpContextClientInfo_ShouldParseLocationAndRequestContextHeaders()
	{
		// Arrange
		var requestDateTime = new DateTime(2026, 10, 25, 12, 30, 0, DateTimeKind.Unspecified);
		var context = CreateHttpContext(IPAddress.Parse("203.0.113.10"), Environments.Development);
		context.Features.Set(new HttpRequestDateTimeFeature { RequestDateTime = requestDateTime });
		context.Request.Headers["Client-Id"] = "client-id";
		context.Request.Headers["X-Client-Latitude"] = "44.4268";
		context.Request.Headers["X-Client-Longitude"] = "26.1025";
		context.Request.Headers["X-Client-Timezone"] = "Europe/Bucharest";
		context.Request.Headers["X-Client-Utc-Offset-Minutes"] = "180";
		context.Request.Headers["X-Client-Language"] = "ro-RO";

		// Act
		var result = context.GetHttpContextClientInfo();

		// Assert
		result.RequestDateTime.Should().Be(requestDateTime);
		result.ClientIP.Should().Be("203.0.113.10");
		result.ClientId.Should().Be("client-id");
		result.ClientLatitude.Should().Be(44.4268);
		result.ClientLongitude.Should().Be(26.1025);
		result.ClientTimeZone.Should().Be("Europe/Bucharest");
		result.ClientUtcOffsetMinutes.Should().Be(180);
		result.ClientLanguage.Should().Be("ro-RO");
	}

	[Fact]
	public void GetHttpContextClientInfo_ShouldDropIncompleteCoordinatePair()
	{
		// Arrange
		var context = CreateHttpContext(IPAddress.Parse("203.0.113.10"), Environments.Development);
		context.Features.Set(new HttpRequestDateTimeFeature { RequestDateTime = DateTime.UtcNow });
		context.Request.Headers["X-Client-Latitude"] = "44.4268";

		// Act
		var result = context.GetHttpContextClientInfo();

		// Assert
		result.ClientLatitude.Should().BeNull();
		result.ClientLongitude.Should().BeNull();
	}

	[Theory]
	[InlineData("91", "26")]
	[InlineData("44", "181")]
	[InlineData("not-a-number", "26")]
	public void GetHttpContextClientInfo_ShouldRejectOutOfRangeCoordinates(string latitude, string longitude)
	{
		// Arrange
		var context = CreateHttpContext(IPAddress.Parse("203.0.113.10"), Environments.Development);
		context.Features.Set(new HttpRequestDateTimeFeature { RequestDateTime = DateTime.UtcNow });
		context.Request.Headers["X-Client-Latitude"] = latitude;
		context.Request.Headers["X-Client-Longitude"] = longitude;

		// Act
		var result = context.GetHttpContextClientInfo();

		// Assert
		result.ClientLatitude.Should().BeNull();
		result.ClientLongitude.Should().BeNull();
	}

	[Theory]
	[InlineData("Europe/Bucharest", "Europe/Bucharest")]
	[InlineData("America/Argentina/Buenos_Aires", "America/Argentina/Buenos_Aires")]
	[InlineData("bad timezone!", null)]
	public void GetClientTimeZone_ShouldOnlyAcceptTimeZoneIdentifiers(string value, string? expected)
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Headers["X-Client-Timezone"] = value;

		// Act
		var result = context.GetClientTimeZone();

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData("180", 180)]
	[InlineData("-300", -300)]
	[InlineData("900", null)]
	[InlineData("abc", null)]
	public void GetClientUtcOffsetMinutes_ShouldOnlyAcceptValidOffsets(string value, int? expected)
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Headers["X-Client-Utc-Offset-Minutes"] = value;

		// Act
		var result = context.GetClientUtcOffsetMinutes();

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void GetClientLanguage_ShouldUseTheFirstAcceptedLanguage()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Headers.AcceptLanguage = "ro-RO,ro;q=0.9,en;q=0.8";

		// Act
		var result = context.GetClientLanguage();

		// Assert
		result.Should().Be("ro-RO");
	}

	[Fact]
	public void GetClientLanguage_ShouldPreferTheExplicitHeader()
	{
		// Arrange
		var context = new DefaultHttpContext();
		context.Request.Headers.AcceptLanguage = "en-US,en;q=0.9";
		context.Request.Headers["X-Client-Language"] = "de-DE";

		// Act
		var result = context.GetClientLanguage();

		// Assert
		result.Should().Be("de-DE");
	}

	private static DefaultHttpContext CreateHttpContext(IPAddress remoteIpAddress, string environmentName)
	{
		var environment = Substitute.For<IWebHostEnvironment>();
		environment.EnvironmentName.Returns(environmentName);

		var services = new ServiceCollection()
			.AddSingleton(environment)
			.BuildServiceProvider();

		return new DefaultHttpContext
		{
			RequestServices = services,
			Connection = { RemoteIpAddress = remoteIpAddress }
		};
	}
}
