using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints;

public sealed class GetSecretMessageEndpoint : EndpointWithoutRequest<GetSecretMessageResponse>
{
	public override void Configure()
	{
		Verbs(Http.GET);
		Routes(Constants.ApiRoutes.GetSecretMessage);
		AllowAnonymous();
	}

	private readonly ISecretMessagesService _secretMessagesService;
	private readonly IGetLogsService _getLogsService;
	private readonly IJwtService _jwtService;
	private readonly ISecretMessageDeliveryNotificationHubService _secretMessageDeliveryNotificationHubService;

	public GetSecretMessageEndpoint(
		ISecretMessagesService secretMessagesService,
		IGetLogsService getLogsService,
		IJwtService jwtService,
		ISecretMessageDeliveryNotificationHubService secretMessageDeliveryNotificationHubService)
	{
		_secretMessagesService = secretMessagesService;
		_getLogsService = getLogsService;
		_jwtService = jwtService;
		_secretMessageDeliveryNotificationHubService = secretMessageDeliveryNotificationHubService;
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var messageId = Route<string>("id")!;

		var result = _secretMessagesService.VerifyExistence(messageId);
		var otpRequired = (result.otp?.Required ?? false);

		if (!result.exists)
		{
			await SendNoContentAsync(cancellation: ct);
			return;
		}

		if (otpRequired)
		{
			var token = HttpContext.ExtractJwtTokenFromRequestHeaders();
			var jwtToken = _jwtService.ValidateToken(token);

			var jwtTokenClaimsAreValid = jwtToken?.Claims.Any(c => c.Type == "messageId" && c.Value == messageId) ?? false;
			if (jwtToken is null || !jwtTokenClaimsAreValid)
			{
				await SendUnauthorizedAsync(cancellation: ct);
				return;
			}
		}

		var secretMessage = await _secretMessagesService.Retrieve(messageId);
		var getLog = await CreateGetLog(messageId, secretMessage);

		var deliveryNotificationSent = await _secretMessageDeliveryNotificationHubService.SendNotification(getLog.ToSecretMessageDeliveryNotification());
		var response = new GetSecretMessageResponse
		{
			DeliveryNotificationSent = deliveryNotificationSent,
			Data = secretMessage!.Data
		};

		await SendOkAsync(response, cancellation: ct);
	}

	private async Task<GetLog> CreateGetLog(string messageId, SecretMessage? secretMessage)
	{
		var getLog = new GetLog
		{
			SecretMessageId = messageId,
			RequestDateTime = HttpContext.GetRequestDateTime(),
			RequestCreatorIP = HttpContext.GetClientIP(),
			RequestClientInfo = HttpContext.GetClientInfo(),
			SecretMessageCreatedDateTime = secretMessage?.CreatedDateTime,
			SecretMessageCreatorIP = secretMessage?.CreatorIP,
			SecretMessageCreatorClientInfo = secretMessage?.CreatorClientInfo
		};

		getLog = await _getLogsService.CreateNewLog(getLog);
		return getLog;
	}
}
