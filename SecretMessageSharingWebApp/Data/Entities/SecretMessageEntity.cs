namespace SecretMessageSharingWebApp.Data.Entities;

public sealed record SecretMessageEntity : IDbEntity
{
	public string Id { get; private init; }

	public DateTime CreatedDateTime { get; init; }

	public string JsonData { get; init; }

	public string? CreatorIP { get; init; }

	public string? CreatorClientInfo { get; init; }

	public OtpSettings? Otp { get; init; }

	public string EncryptionKeySha256 { get; init; }


	public SecretMessageEntity()
	{
		Id = Guid.NewGuid().ToString("N");
	}
}

public sealed record OtpSettings
{
	public bool Required { get; private set; } = true;

	public string RecipientsEmail { get; init; }
}
