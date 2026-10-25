using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using SecretMessageSharingWebApp.ConfigurationSettings;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services;

public sealed class RecipientLocationService(
	HttpClient httpClient,
	IMemoryCache memoryCache,
	RecipientLocationConfigurationSettings settings,
	ILogger<RecipientLocationService> logger) : IRecipientLocationService
{
	private const string IpCacheKeyPrefix = "recipient-location:ip:";

	public async Task<RecipientLocation?> ResolveAsync(
		string? ipAddress,
		double? latitude,
		double? longitude,
		CancellationToken cancellationToken = default)
	{
		if (settings.Enabled is false)
		{
			return null;
		}

		using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeoutCts.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));

		var hasCoordinates =
			latitude is >= -90 and <= 90 &&
			longitude is >= -180 and <= 180;

		// Run both lookups in parallel: the device lookup is preferred, while the IP lookup
		// supplies the ISP and remains the fallback when the device lookup fails.
		var ipTask = ResolveFromIpAsync(ipAddress, timeoutCts.Token);
		var deviceTask = hasCoordinates
			? ResolveFromCoordinatesAsync(latitude!.Value, longitude!.Value, timeoutCts.Token)
			: Task.FromResult<RecipientLocation?>(null);

		await Task.WhenAll(ipTask, deviceTask).ConfigureAwait(false);

		var ipLocation = await ipTask.ConfigureAwait(false);
		var deviceLocation = await deviceTask.ConfigureAwait(false);

		if (deviceLocation is not null)
		{
			return deviceLocation with { Isp = ipLocation?.Isp };
		}

		return ipLocation;
	}

	private async Task<RecipientLocation?> ResolveFromCoordinatesAsync(
		double latitude,
		double longitude,
		CancellationToken cancellationToken)
	{
		try
		{
			var endpoint = settings.ReverseGeocodeEndpoint;
			var separator = endpoint.Contains('?') ? '&' : '?';

			// Coordinates are rounded before they leave the browser and are only used
			// for this lookup. They are never logged or persisted.
			var url = string.Concat(
				endpoint,
				separator,
				"latitude=",
				latitude.ToString("0.###", CultureInfo.InvariantCulture),
				"&longitude=",
				longitude.ToString("0.###", CultureInfo.InvariantCulture),
				"&localityLanguage=en");

			using var response = await httpClient
				.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
				.ConfigureAwait(false);

			if (response.IsSuccessStatusCode is false)
			{
				logger.LogDebug(
					"Recipient city lookup from device coordinates failed with status {StatusCode}.",
					(int)response.StatusCode);

				return null;
			}

			var dto = await response.Content
				.ReadFromJsonAsync<ReverseGeocodeResponse>(cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			var city = Clean(dto?.City) ?? Clean(dto?.Locality);
			if (city is null)
			{
				return null;
			}

			return new RecipientLocation
			{
				City = city,
				Region = Clean(dto?.PrincipalSubdivision),
				Country = Clean(dto?.CountryName),
				CountryCode = Clean(dto?.CountryCode)?.ToUpperInvariant(),
				Source = Constants.LocationSources.Device
			};
		}
		catch (OperationCanceledException)
		{
			return null;
		}
		catch (Exception exception) when (exception is HttpRequestException or JsonException or NotSupportedException)
		{
			// Do not log the request URL: it contains the recipient's coordinates.
			logger.LogWarning(
				"Recipient city lookup from device coordinates failed ({ExceptionType}).",
				exception.GetType().Name);

			return null;
		}
	}

	private async Task<RecipientLocation?> ResolveFromIpAsync(
		string? ipAddress,
		CancellationToken cancellationToken)
	{
		var lookupAddress = ipAddress;

		if (IsPublicIp(lookupAddress) is false)
		{
			if (settings.ResolveLocalRequestsThroughPublicIp is false || IsLocalRequest(lookupAddress) is false)
			{
				return null;
			}

			// The request came from the developer's own machine (or has no address at
			// all), so there is no public IP of its own to look up. When the provider is
			// called without an address it resolves the caller's egress IP instead.
			lookupAddress = null;
		}

		var cacheKey = IpCacheKeyPrefix + (lookupAddress ?? "caller");
		if (memoryCache.TryGetValue(cacheKey, out RecipientLocation? cachedLocation))
		{
			return cachedLocation;
		}

		try
		{
			var endpoint = settings.IpGeolocationEndpoint.TrimEnd('/');
			var url = lookupAddress is null
				? string.Concat(endpoint, "/?lang=en")
				: string.Concat(
					endpoint,
					"/",
					Uri.EscapeDataString(lookupAddress),
					"?lang=en");

			using var response = await httpClient
				.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
				.ConfigureAwait(false);

			if (response.IsSuccessStatusCode is false)
			{
				logger.LogDebug(
					"Recipient city lookup from IP failed with status {StatusCode}.",
					(int)response.StatusCode);

				return null;
			}

			var dto = await response.Content
				.ReadFromJsonAsync<IpGeolocationResponse>(cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (dto?.Success is not true)
			{
				return null;
			}

			var city = Clean(dto.City) ?? Clean(dto.Region);
			if (city is null)
			{
				return null;
			}

			var result = new RecipientLocation
			{
				City = city,
				Region = Clean(dto.Region),
				Country = Clean(dto.Country),
				CountryCode = Clean(dto.CountryCode)?.ToUpperInvariant(),
				Isp = Clean(dto.Connection?.Isp ?? dto.Connection?.Org),
				Source = Constants.LocationSources.Ip
			};

			memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(settings.IpCacheMinutes));

			return result;
		}
		catch (OperationCanceledException)
		{
			return null;
		}
		catch (Exception exception) when (exception is HttpRequestException or JsonException or NotSupportedException)
		{
			logger.LogWarning(
				"Recipient city lookup from IP failed ({ExceptionType}).",
				exception.GetType().Name);

			return null;
		}
	}

	private static bool IsLocalRequest(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ||
			string.Equals(value, "localhost", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(value, "Unknown", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsPublicIp(string? value)
	{
		if (IPAddress.TryParse(value, out var address) is false)
		{
			return false;
		}

		if (address.IsIPv4MappedToIPv6)
		{
			address = address.MapToIPv4();
		}

		if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
		{
			return false;
		}

		if (address.AddressFamily == AddressFamily.InterNetwork)
		{
			var bytes = address.GetAddressBytes();

			return bytes[0] switch
			{
				0 => false,
				10 => false,
				100 when bytes[1] is >= 64 and <= 127 => false, // CGNAT
				127 => false,
				169 when bytes[1] == 254 => false,
				172 when bytes[1] is >= 16 and <= 31 => false,
				192 when bytes[1] == 168 => false,
				192 when bytes[1] == 0 && bytes[2] == 0 => false,
				198 when bytes[1] is 18 or 19 => false,
				198 when bytes[1] == 51 && bytes[2] == 100 => false,
				203 when bytes[1] == 0 && bytes[2] == 113 => false,
				_ => true
			};
		}

		if (address.AddressFamily == AddressFamily.InterNetworkV6)
		{
			if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast)
			{
				return false;
			}

			var bytes = address.GetAddressBytes();

			// fc00::/7 - unique local addresses.
			return (bytes[0] & 0xFE) != 0xFC;
		}

		return false;
	}

	private static string? Clean(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		var trimmed = value.Trim();

		return trimmed.Length <= 120 ? trimmed : trimmed[..120];
	}

	private sealed record ReverseGeocodeResponse
	{
		[JsonPropertyName("city")]
		public string? City { get; init; }

		[JsonPropertyName("locality")]
		public string? Locality { get; init; }

		[JsonPropertyName("principalSubdivision")]
		public string? PrincipalSubdivision { get; init; }

		[JsonPropertyName("countryName")]
		public string? CountryName { get; init; }

		[JsonPropertyName("countryCode")]
		public string? CountryCode { get; init; }
	}

	private sealed record IpGeolocationResponse
	{
		[JsonPropertyName("success")]
		public bool Success { get; init; }

		[JsonPropertyName("city")]
		public string? City { get; init; }

		[JsonPropertyName("region")]
		public string? Region { get; init; }

		[JsonPropertyName("country")]
		public string? Country { get; init; }

		[JsonPropertyName("country_code")]
		public string? CountryCode { get; init; }

		[JsonPropertyName("connection")]
		public IpGeolocationConnection? Connection { get; init; }
	}

	private sealed record IpGeolocationConnection
	{
		[JsonPropertyName("isp")]
		public string? Isp { get; init; }

		[JsonPropertyName("org")]
		public string? Org { get; init; }
	}
}
