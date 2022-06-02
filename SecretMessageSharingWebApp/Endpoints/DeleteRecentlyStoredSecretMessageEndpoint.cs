using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints
{
	public class DeleteRecentlyStoredSecretMessageEndpoint : EndpointWithoutRequest
	{
		public override void Configure()
		{
			Verbs(Http.DELETE);
			Routes("/api/secret-messages/deleteRecentlyStoredSecretMessage/{id}");
			AllowAnonymous();
		}

		private readonly ISecretMessagesService _secretMessagesService;

		public DeleteRecentlyStoredSecretMessageEndpoint(ISecretMessagesService secretMessagesService)
		{
			_secretMessagesService = secretMessagesService;
		}

		public override async Task HandleAsync(CancellationToken ct)
		{
			var id = Route<string>("id")!;

			var recentStoredSecretMessagesList = GetRecentlyStoredSecretMessagesList();
			if (!recentStoredSecretMessagesList.Contains(id))
			{
				await SendAsync(false, cancellation: ct);
				return;
			}

			var deleted = await _secretMessagesService.Delete(id);
			await SendAsync(deleted, cancellation: ct);
		}

		private List<string> GetRecentlyStoredSecretMessagesList()
		{
			return HttpContext.Session.GetObject<List<string>>(Constants.SessionKey_RecentlyStoredSecretMessagesList) ?? new();
		}
	}
}
