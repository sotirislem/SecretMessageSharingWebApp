using FastEndpoints;
using SecretMessageSharingWebApp.Extensions;
using SecretMessageSharingWebApp.Mappings;
using SecretMessageSharingWebApp.Models.Api.Requests;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Endpoints
{
	public class VerifySecretMessageEndpoint : EndpointWithoutRequest<VerifySecretMessageResponse>
	{
		public override void Configure()
		{
			Verbs(Http.GET);
			Routes("/api/secret-messages/verify/{id}");
			AllowAnonymous();
		}

		private readonly ISecretMessagesService _secretMessagesService;

		public VerifySecretMessageEndpoint(ISecretMessagesService secretMessagesService)
		{
			_secretMessagesService = secretMessagesService;
		}

		public override async Task HandleAsync(CancellationToken ct)
        {
			var messageId = Route<string>("id")!;

			var result = _secretMessagesService.VerifyExistence(messageId);

			var response = new VerifySecretMessageResponse
			{
				Id = messageId,
				Exists = result.exists,
				RequiresOtp = result.otp?.Required
			};

			await SendOkAsync(response, cancellation: ct);
		}
	}
}
