using SecretMessageSharingWebApp.Models.Common;

namespace SecretMessageSharingWebApp.Models.Api.Requests;

public sealed class StoreNewSecretMessageRequest
{
	public SecretMessageData SecretMessageData { get; init; }

	public OtpSettings Otp { get; init; }
}

public sealed class OtpSettings
{
	public bool Required { get; init; }

	public string RecipientsEmail { get; init; }
}
