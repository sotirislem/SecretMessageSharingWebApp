using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints
{
	public class GetSecretMessageEndpoint : EndpointWithoutRequest<GetSecretMessageResponse>
	{
		public override void Configure()
		{
			Verbs(Http.GET);
			Routes("/api/secret-messages/get/{id}");
			AllowAnonymous();
		}

		private readonly ISecretMessagesService _secretMessagesService;
		private readonly IGetLogsService _getLogsService;
		private readonly ISecretMessageDeliveryNotificationHubService _secretMessageDeliveryNotificationHubService;

		public GetSecretMessageEndpoint(ISecretMessagesService secretMessagesService, IGetLogsService getLogsService, ISecretMessageDeliveryNotificationHubService secretMessageDeliveryNotificationHubService)
		{
			_secretMessagesService = secretMessagesService;
			_getLogsService = getLogsService;
			_secretMessageDeliveryNotificationHubService = secretMessageDeliveryNotificationHubService;
		}

		public override async Task HandleAsync(CancellationToken ct)
        {
			var id = Route<string>("id")!;

			var secretMessage = await _secretMessagesService.Retrieve(id);

			var getLog = new GetLog
			{
				SecretMessageId = id,
				RequestDateTime = HttpContext.GetRequestDateTime(),
				RequestCreatorIP = HttpContext.GetClientIP(),
				RequestClientInfo = HttpContext.GetClientInfo(),
				SecretMessageExisted = (secretMessage is not null),
				SecretMessageCreatedDateTime = secretMessage?.CreatedDateTime,
				SecretMessageCreatorIP = secretMessage?.CreatorIP,
				SecretMessageCreatorClientInfo = secretMessage?.CreatorClientInfo
			};
			getLog = _getLogsService.CreateNewLog(getLog);

			if (secretMessage is null)
			{
				await SendNoContentAsync(ct);
				return;
			}

			var deliveryNotificationSent = await _secretMessageDeliveryNotificationHubService.Send(getLog.ToSecretMessageDeliveryNotification());
			var response = new GetSecretMessageResponse
			{
				DeliveryNotificationSent = deliveryNotificationSent,
				Data = secretMessage.Data
			};

			await SendAsync(response, cancellation: ct);
		}
	}
}
