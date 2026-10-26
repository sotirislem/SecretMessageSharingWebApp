using System.Net.Http.Headers;
using Polly;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SecretMessageSharingWebApp.Providers;

/// <summary>
/// Applies the resilience pipeline to every SendGrid API call, so callers get
/// retries and per-attempt timeouts without depending on Polly directly.
/// </summary>
public sealed class ResilientSendGridClient(
	ISendGridClient innerClient,
	ResiliencePipeline<Response> resiliencePipeline) : ISendGridClient
{
	public string UrlPath
	{
		get => innerClient.UrlPath;
		set => innerClient.UrlPath = value;
	}

	public string Version
	{
		get => innerClient.Version;
		set => innerClient.Version = value;
	}

	public string MediaType
	{
		get => innerClient.MediaType;
		set => innerClient.MediaType = value;
	}

	public AuthenticationHeaderValue AddAuthorization(KeyValuePair<string, string> header) =>
		innerClient.AddAuthorization(header);

	public Task<Response> MakeRequest(HttpRequestMessage request, CancellationToken cancellationToken = default) =>
		ExecuteAsync(token => innerClient.MakeRequest(request, token), cancellationToken);

	public Task<Response> RequestAsync(
		BaseClient.Method method,
		string? requestBody = null,
		string? queryParams = null,
		string? urlPath = null,
		CancellationToken cancellationToken = default) =>
		ExecuteAsync(
			token => innerClient.RequestAsync(method, requestBody, queryParams, urlPath, token),
			cancellationToken);

	public Task<Response> SendEmailAsync(SendGridMessage msg, CancellationToken cancellationToken = default) =>
		ExecuteAsync(token => innerClient.SendEmailAsync(msg, token), cancellationToken);

	private async Task<Response> ExecuteAsync(
		Func<CancellationToken, Task<Response>> action,
		CancellationToken cancellationToken)
	{
		return await resiliencePipeline.ExecuteAsync(
			token => new ValueTask<Response>(action(token)),
			cancellationToken);
	}
}
