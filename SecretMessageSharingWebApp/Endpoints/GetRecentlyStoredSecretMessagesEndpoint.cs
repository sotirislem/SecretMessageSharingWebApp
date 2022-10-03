using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class GetRecentlyStoredSecretMessagesEndpoint : EndpointWithoutRequest<RecentlyStoredSecretMessagesResponse>
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes(Constants.ApiRoutes.GetRecentlyStoredSecretMessages);
		AllowAnonymous();
	}

	private readonly ISecretMessagesService _secretMessagesService;
	private readonly IGetLogsService _getLogsService;

	public GetRecentlyStoredSecretMessagesEndpoint(ISecretMessagesService secretMessagesService, IGetLogsService getLogsService)
	{
		_secretMessagesService = secretMessagesService;
		_getLogsService = getLogsService;
	}

	public override async Task HandleAsync(CancellationToken ct)
        {
		var recentStoredSecretMessagesList = GetRecentlyStoredSecretMessagesList();
		if (!recentStoredSecretMessagesList.Any())
		{
			await SendAsync(new RecentlyStoredSecretMessagesResponse(), cancellation: ct);
			return;
		}

		var resultsPartA = _secretMessagesService.GetRecentlyStoredSecretMessagesInfo(recentStoredSecretMessagesList);
		var resultsPartB = _getLogsService.GetRecentlyStoredSecretMessagesInfo(recentStoredSecretMessagesList);

		var recentlyStoredSecretMessages = resultsPartA.Union(resultsPartB)
			.OrderByDescending(x => x.CreatedDateTime)
			.Select(x => x.ToApiRecentlyStoredSecretMessage())
			.ToList();

		var response = new RecentlyStoredSecretMessagesResponse()
		{
			RecentlyStoredSecretMessages = recentlyStoredSecretMessages
		};
		await SendAsync(response, cancellation: ct);
	}

	private List<string> GetRecentlyStoredSecretMessagesList()
	{
		return HttpContext.Session.GetObject<List<string>>(Constants.SessionKey_RecentlyStoredSecretMessagesList) ?? new();
	}
}
