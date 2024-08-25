namespace SecretMessageSharingWebApp.Extensions;

public static class StringExtensions
{
	public static string? MaskIpAddress(this string? ipAddress)
	{
		if (string.IsNullOrWhiteSpace(ipAddress))
		{
			return null;
		}

		if (ipAddress.Contains('.') is false)
		{
			return ipAddress;
		}

		return string.Concat(ipAddress.AsSpan(0, ipAddress!.LastIndexOf('.')), ".xxx");
	}
}
