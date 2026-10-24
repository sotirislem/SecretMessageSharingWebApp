using System.Security.Cryptography;
using System.Text;

namespace SecretMessageSharingWebApp.Models.Domain;

public sealed record OneTimePassword
{
	public string Code { get; init; }

	public DateTimeOffset ExpiresAt { get; init; }

	private int _codeValidationAttempts;

	public int CodeValidationAttempts => Volatile.Read(ref _codeValidationAttempts);

	public bool Validate(string codeToValidate)
	{
		Interlocked.Increment(ref _codeValidationAttempts);

		if (string.IsNullOrEmpty(Code) || string.IsNullOrEmpty(codeToValidate))
		{
			return false;
		}

		var expectedBytes = Encoding.UTF8.GetBytes(Code);
		var providedBytes = Encoding.UTF8.GetBytes(codeToValidate);

		return expectedBytes.Length == providedBytes.Length
			&& CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
	}
}
