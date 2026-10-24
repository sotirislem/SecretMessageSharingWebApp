using System.Security.Cryptography;
using System.Text;

namespace SecretMessageSharingWebApp.Security;

/// <summary>
/// Creates and verifies the value stored for a message's encryption-key hash.
/// Records store a domain-separated hash of the client-provided key hash, so a
/// read-only database leak cannot be replayed directly against the retrieval API.
/// </summary>
public static class KeyHashVerifier
{
	private const string DomainSeparator = "SecretMessageSharingWebApp:key-hash-verifier:";

	public static string Create(string keyHash)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(keyHash);

		// Hex digests are case-insensitive; normalize so clients that send uppercase
		// hex can still retrieve messages created with lowercase hex (and vice versa).
		var digest = SHA256.HashData(Encoding.UTF8.GetBytes(DomainSeparator + keyHash.ToLowerInvariant()));

		return Convert.ToHexString(digest).ToLowerInvariant();
	}

	public static bool Matches(string storedVerifier, string? providedKeyHash)
	{
		if (string.IsNullOrWhiteSpace(storedVerifier) || string.IsNullOrWhiteSpace(providedKeyHash))
		{
			return false;
		}

		var expected = Create(providedKeyHash);

		return FixedTimeEquals(storedVerifier.ToLowerInvariant(), expected);
	}

	private static bool FixedTimeEquals(string left, string right)
	{
		var leftBytes = Encoding.ASCII.GetBytes(left);
		var rightBytes = Encoding.ASCII.GetBytes(right);

		return leftBytes.Length == rightBytes.Length
			&& CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
	}
}
