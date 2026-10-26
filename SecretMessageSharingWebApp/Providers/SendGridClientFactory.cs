using Polly;
using SendGrid;

namespace SecretMessageSharingWebApp.Providers;

public interface ISendGridClientFactory
{
	ISendGridClient Create(string apiKey);
}

public sealed class SendGridClientFactory(
	IHttpClientFactory httpClientFactory,
	ResiliencePipeline<Response> resiliencePipeline) : ISendGridClientFactory
{
	public const string HttpClientName = "SendGrid";

	public ISendGridClient Create(string apiKey)
	{
		// Reuse the pooled handler from IHttpClientFactory instead of letting
		// SendGridClient create a new HttpClient and socket pool for every email.
		var httpClient = httpClientFactory.CreateClient(HttpClientName);

		var client = new SendGridClient(httpClient, new SendGridClientOptions { ApiKey = apiKey });

		return new ResilientSendGridClient(client, resiliencePipeline);
	}
}
