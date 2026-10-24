using System.Text.Json.Serialization;

namespace SecretMessageSharingWebApp.Models.Common;

public sealed record SecretMessageData
{
	[JsonPropertyName("iv")]
	public string IV { get; init; }

	[JsonPropertyName("ct")]
	public string CT { get; init; }
}
