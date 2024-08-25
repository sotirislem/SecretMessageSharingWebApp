namespace SecretMessageSharingWebApp.Models.Domain;

public sealed record OneTimePassword
{
	public string Code { get; init; }

	public DateTimeOffset ExpiresAt { get; init; }

	public int CodeValidationAttempts { get; private set; } = 0;


	public bool Validate(string codeToValidate)
	{
		CodeValidationAttempts++;

		return codeToValidate == Code;
	}
}