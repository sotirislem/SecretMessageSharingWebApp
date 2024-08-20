namespace SecretMessageSharingWebApp.Models.Domain;

public sealed record OneTimePassword
{
	private readonly string _code;

	public string Code
	{
		get
		{
			TotalCodeAccesses++;

			return _code;
		}
		init => _code = value;
	}

	public long CreatedTimestamp { get; init; }

	public int TotalCodeAccesses { get; private set; } = 0;
}