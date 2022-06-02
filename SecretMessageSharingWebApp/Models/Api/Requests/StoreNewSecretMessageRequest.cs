using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests
{
	public class StoreNewSecretMessageRequest
	{
		public SecretMessageData Data { get; init; }
	}
}
