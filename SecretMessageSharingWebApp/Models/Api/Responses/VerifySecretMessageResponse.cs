namespace SecretMessageSharingWebApp.Models.Api.Responses
{
	public class VerifySecretMessageResponse
	{
		public string Id { get; init; }

		public bool Exists { get; init; }

		public bool? RequiresOtp { get; init; }
	}
}
