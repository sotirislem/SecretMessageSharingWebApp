using System.Net;
using System.Net.Sockets;

namespace SecretMessageSharingWebApp.Extensions;

public static class StringExtensions
{
	public static string? MaskIpAddress(this string? ipAddress)
	{
		if (string.IsNullOrWhiteSpace(ipAddress))
		{
			return null;
		}

		if (IPAddress.TryParse(ipAddress, out var parsedAddress) is false)
		{
			return ipAddress;
		}

		if (parsedAddress.IsIPv4MappedToIPv6)
		{
			parsedAddress = parsedAddress.MapToIPv4();
		}

		if (parsedAddress.AddressFamily == AddressFamily.InterNetwork)
		{
			var ipv4 = parsedAddress.ToString();
			return string.Concat(ipv4.AsSpan(0, ipv4.LastIndexOf('.')), ".xxx");
		}

		if (parsedAddress.AddressFamily == AddressFamily.InterNetworkV6)
		{
			// Mask the interface identifier (the lower 64 bits) so only the network prefix
			// is visible, mirroring the IPv4 masking above.
			var bytes = parsedAddress.GetAddressBytes();
			Array.Clear(bytes, 8, 8);

			return new IPAddress(bytes) + "/64";
		}

		return ipAddress;
	}
}
