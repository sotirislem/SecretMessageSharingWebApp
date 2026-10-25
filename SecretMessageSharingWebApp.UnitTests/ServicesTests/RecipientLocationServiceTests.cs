using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SecretMessageSharingWebApp.ConfigurationSettings;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class RecipientLocationServiceTests
{
	private const string ReverseGeocodeEndpoint = "https://geo.example.com/reverse-geocode-client";
	private const string IpGeolocationEndpoint = "https://ip.example.com";

	private const string DeviceResponse = """
		{
			"city": "Bucharest",
			"principalSubdivision": "Bucuresti",
			"countryName": "Romania",
			"countryCode": "RO"
		}
		""";

	private const string IpResponse = """
		{
			"success": true,
			"city": "Ploiesti",
			"region": "Prahova",
			"country": "Romania",
			"country_code": "RO",
			"connection": { "isp": "DIGI Romania" }
		}
		""";

	private readonly MemoryCache _memoryCache;

	public RecipientLocationServiceTests()
	{
		_memoryCache = new MemoryCache(new MemoryCacheOptions());
	}

	[Fact]
	public async Task ResolveAsync_ShouldPreferDeviceCity_AndAttachIspFromIpLookup()
	{
		// Arrange
		var handler = CreateHandler(request => request.RequestUri!.Host == "geo.example.com"
			? JsonResponse(HttpStatusCode.OK, DeviceResponse)
			: JsonResponse(HttpStatusCode.OK, IpResponse));

		var sut = CreateService(handler);

		// Act
		var result = await sut.ResolveAsync("8.8.8.8", 44.4268, 26.1025);

		// Assert
		result.Should().NotBeNull();
		result!.City.Should().Be("Bucharest");
		result.Region.Should().Be("Bucuresti");
		result.Country.Should().Be("Romania");
		result.CountryCode.Should().Be("RO");
		result.Isp.Should().Be("DIGI Romania");
		result.Source.Should().Be(Constants.LocationSources.Device);

		handler.Requests.Should().HaveCount(2);
		handler.Requests.Should().Contain(request =>
			request.Host == "geo.example.com" &&
			request.Query.Contains("latitude=44.427") &&
			request.Query.Contains("longitude=26.103"));
	}

	[Fact]
	public async Task ResolveAsync_ShouldFallBackToIpCity_WhenReverseGeocodingFails()
	{
		// Arrange
		var handler = CreateHandler(request => request.RequestUri!.Host == "geo.example.com"
			? new HttpResponseMessage(HttpStatusCode.InternalServerError)
			: JsonResponse(HttpStatusCode.OK, IpResponse));

		var sut = CreateService(handler);

		// Act
		var result = await sut.ResolveAsync("8.8.8.8", 44.4268, 26.1025);

		// Assert
		result.Should().NotBeNull();
		result!.City.Should().Be("Ploiesti");
		result.Source.Should().Be(Constants.LocationSources.Ip);
	}

	[Fact]
	public async Task ResolveAsync_ShouldUseIpCity_WhenNoCoordinatesWereProvided()
	{
		// Arrange
		var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, IpResponse));
		var sut = CreateService(handler);

		// Act
		var result = await sut.ResolveAsync("8.8.8.8", null, null);

		// Assert
		result.Should().NotBeNull();
		result!.City.Should().Be("Ploiesti");
		result.Source.Should().Be(Constants.LocationSources.Ip);
		handler.Requests.Should().ContainSingle();
	}

	[Fact]
	public async Task ResolveAsync_ShouldIgnoreOutOfRangeCoordinates()
	{
		// Arrange
		var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, IpResponse));
		var sut = CreateService(handler);

		// Act
		var result = await sut.ResolveAsync("8.8.8.8", 200, 500);

		// Assert
		result.Should().NotBeNull();
		result!.Source.Should().Be(Constants.LocationSources.Ip);
		handler.Requests.Should().ContainSingle(request => request.Host == "ip.example.com");
	}

	[Fact]
	public async Task ResolveAsync_ShouldNotLookUpPrivateIpAddresses()
	{
		// Arrange
		var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, DeviceResponse));
		var sut = CreateService(handler);

		// Act
		var result = await sut.ResolveAsync("192.168.1.5", 44.4268, 26.1025);

		// Assert
		result.Should().NotBeNull();
		result!.Source.Should().Be(Constants.LocationSources.Device);
		handler.Requests.Should().ContainSingle(request => request.Host == "geo.example.com");
	}

	[Fact]
	public async Task ResolveAsync_ShouldCacheIpLookups()
	{
		// Arrange
		var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, IpResponse));
		var sut = CreateService(handler);

		// Act
		await sut.ResolveAsync("8.8.8.8", null, null);
		var secondResult = await sut.ResolveAsync("8.8.8.8", null, null);

		// Assert
		secondResult.Should().NotBeNull();
		handler.Requests.Should().ContainSingle();
	}

	[Fact]
	public async Task ResolveAsync_ShouldResolveTheEgressCity_ForLocalRequestsWhenEnabled()
	{
		// Arrange
		var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, IpResponse));
		var sut = CreateService(handler, resolveLocalRequestsThroughPublicIp: true);

		// Act
		var result = await sut.ResolveAsync("localhost", null, null);

		// Assert
		result.Should().NotBeNull();
		result!.City.Should().Be("Ploiesti");
		result.Source.Should().Be(Constants.LocationSources.Ip);
		handler.Requests.Should().ContainSingle();
		handler.Requests[0].AbsolutePath.Should().Be("/");
		handler.Requests[0].Query.Should().Be("?lang=en");
	}

	[Fact]
	public async Task ResolveAsync_ShouldNotResolveTheEgressCity_ForLocalRequestsWhenDisabled()
	{
		// Arrange
		var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, IpResponse));
		var sut = CreateService(handler);

		// Act
		var result = await sut.ResolveAsync("localhost", null, null);

		// Assert
		result.Should().BeNull();
		handler.Requests.Should().BeEmpty();
	}

	[Fact]
	public async Task ResolveAsync_ShouldNotResolveTheEgressCity_ForPrivateLanAddresses()
	{
		// Arrange
		var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, IpResponse));
		var sut = CreateService(handler, resolveLocalRequestsThroughPublicIp: true);

		// Act
		var result = await sut.ResolveAsync("192.168.1.5", null, null);

		// Assert
		result.Should().BeNull();
		handler.Requests.Should().BeEmpty();
	}

	[Fact]
	public async Task ResolveAsync_ShouldReturnNull_WhenDisabled()
	{
		// Arrange
		var handler = CreateHandler(_ => JsonResponse(HttpStatusCode.OK, IpResponse));
		var sut = CreateService(handler, enabled: false);

		// Act
		var result = await sut.ResolveAsync("8.8.8.8", 44.4268, 26.1025);

		// Assert
		result.Should().BeNull();
		handler.Requests.Should().BeEmpty();
	}

	[Fact]
	public async Task ResolveAsync_ShouldReturnNull_WhenBothProvidersFail()
	{
		// Arrange
		var handler = CreateHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
		var sut = CreateService(handler);

		// Act
		var result = await sut.ResolveAsync("8.8.8.8", 44.4268, 26.1025);

		// Assert
		result.Should().BeNull();
	}

	private RecipientLocationService CreateService(
		StubHttpMessageHandler handler,
		bool enabled = true,
		bool resolveLocalRequestsThroughPublicIp = false)
	{
		var settings = new RecipientLocationConfigurationSettings
		{
			Enabled = enabled,
			ReverseGeocodeEndpoint = ReverseGeocodeEndpoint,
			IpGeolocationEndpoint = IpGeolocationEndpoint,
			TimeoutSeconds = 2,
			IpCacheMinutes = 60,
			ResolveLocalRequestsThroughPublicIp = resolveLocalRequestsThroughPublicIp
		};

		return new RecipientLocationService(
			new HttpClient(handler),
			_memoryCache,
			settings,
			Substitute.For<ILogger<RecipientLocationService>>());
	}

	private static StubHttpMessageHandler CreateHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
	{
		return new StubHttpMessageHandler(handler);
	}

	private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json)
	{
		return new HttpResponseMessage(statusCode)
		{
			Content = new StringContent(json, Encoding.UTF8, "application/json")
		};
	}

	private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
	{
		public List<Uri> Requests { get; } = [];

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Requests.Add(request.RequestUri!);

			return Task.FromResult(handler(request));
		}
	}
}
