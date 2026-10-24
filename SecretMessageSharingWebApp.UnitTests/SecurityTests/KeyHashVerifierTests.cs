using System.Security.Cryptography;
using System.Text;
using SecretMessageSharingWebApp.Security;

namespace SecretMessageSharingWebApp.UnitTests.SecurityTests;

public class KeyHashVerifierTests
{
	private readonly IFixture _fixture;

	public KeyHashVerifierTests()
	{
		_fixture = new Fixture();
	}

	[Fact]
	public void Create_ShouldReturnDeterministicDigest()
	{
		// Arrange
		var keyHash = CreateKeyHash();

		// Act
		var first = KeyHashVerifier.Create(keyHash);
		var second = KeyHashVerifier.Create(keyHash);

		// Assert
		first.Should().MatchRegex("^[a-f0-9]{64}$");
		first.Should().HaveLength(64);
		first.Should().Be(second);
		first.Should().NotContain(keyHash);
	}

	[Fact]
	public void Create_ShouldThrow_WhenKeyHashIsMissing()
	{
		// Act
		var act = () => KeyHashVerifier.Create(" ");

		// Assert
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Matches_ShouldReturnTrue_ForStoredDigest()
	{
		// Arrange
		var keyHash = CreateKeyHash();
		var storedVerifier = KeyHashVerifier.Create(keyHash);

		// Act / Assert
		KeyHashVerifier.Matches(storedVerifier, keyHash).Should().BeTrue();
	}

	[Fact]
	public void Create_ShouldTreatHexDigestCaseInsensitively()
	{
		// Arrange
		var keyHash = CreateKeyHash();

		// Act / Assert
		KeyHashVerifier.Create(keyHash.ToUpperInvariant()).Should().Be(KeyHashVerifier.Create(keyHash));
		KeyHashVerifier.Matches(KeyHashVerifier.Create(keyHash.ToUpperInvariant()), keyHash).Should().BeTrue();
	}

	[Fact]
	public void Matches_ShouldReturnFalse_ForRawKeyHash()
	{
		// Arrange
		var keyHash = CreateKeyHash();

		// Act / Assert
		// Stored values are always domain-separated digests, never the raw key hash.
		KeyHashVerifier.Matches(keyHash, keyHash).Should().BeFalse();
		KeyHashVerifier.Matches(keyHash.ToUpperInvariant(), keyHash).Should().BeFalse();
	}

	[Fact]
	public void Matches_ShouldReturnFalse_ForWrongKeyHash()
	{
		// Arrange
		var storedVerifier = KeyHashVerifier.Create(CreateKeyHash());

		// Act / Assert
		KeyHashVerifier.Matches(storedVerifier, CreateKeyHash()).Should().BeFalse();
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Matches_ShouldReturnFalse_WhenProvidedKeyHashIsMissing(string? providedKeyHash)
	{
		// Arrange
		var storedVerifier = KeyHashVerifier.Create(CreateKeyHash());

		// Act / Assert
		KeyHashVerifier.Matches(storedVerifier, providedKeyHash).Should().BeFalse();
	}

	[Fact]
	public void Matches_ShouldReturnFalse_WhenStoredVerifierIsMissing()
	{
		// Act / Assert
		KeyHashVerifier.Matches("", CreateKeyHash()).Should().BeFalse();
	}

	private string CreateKeyHash()
	{
		var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(_fixture.Create<string>()));

		return Convert.ToHexString(hashBytes).ToLowerInvariant();
	}
}
