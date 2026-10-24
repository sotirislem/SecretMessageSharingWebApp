using SendGrid;

namespace SecretMessageSharingWebApp.Providers;

public interface ISendGridClientFactory
{
	ISendGridClient Create(string apiKey);
}

public sealed class SendGridClientFactory : ISendGridClientFactory
{
	public ISendGridClient Create(string apiKey) => new SendGridClient(apiKey);
}
