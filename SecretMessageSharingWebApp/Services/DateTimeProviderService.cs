using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services;

public sealed class DateTimeProviderService : IDateTimeProviderService
{
	public DateTime LocalNow()
	{
		var destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(Constants.LocalTimeZone);

		return TimeZoneInfo.ConvertTime(DateTime.Now, destinationTimeZone);
	}

	public DateTime UtcNow()
	{
		return DateTime.UtcNow;
	}

	public long UtcNowUnixTimeSeconds()
	{
		return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
	}
}
