using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services;

public sealed class DateTimeProviderService : IDateTimeProviderService
{
	private const string LocalTimeZoneId = "GTB Standard Time";

	public DateTime LocalNow()
	{
		var destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(LocalTimeZoneId);

		return TimeZoneInfo.ConvertTime(DateTime.UtcNow, destinationTimeZone);
	}

	public DateTime UtcNow()
	{
		return DateTime.UtcNow;
	}
}
