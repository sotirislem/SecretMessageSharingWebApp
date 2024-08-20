namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface IDateTimeProviderService
{
	DateTime LocalNow();

	DateTime UtcNow();

	long UtcNowUnixTimeSeconds();
}