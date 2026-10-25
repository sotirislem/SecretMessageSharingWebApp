using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface IRecipientLocationService
{
	/// <summary>
	/// Resolves a city from the recipient's device coordinates and/or IP address.
	/// Device coordinates are preferred; the IP lookup is both the fallback and the
	/// source of the ISP name. Coordinates are never persisted.
	/// </summary>
	Task<RecipientLocation?> ResolveAsync(
		string? ipAddress,
		double? latitude,
		double? longitude,
		CancellationToken cancellationToken = default);
}
