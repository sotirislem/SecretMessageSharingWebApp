using Microsoft.Extensions.Caching.Memory;
using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class MemoryCacheServiceTests : IDisposable
{
	private readonly MemoryCache _memoryCache;
	private readonly MemoryCacheService _sut;

	public MemoryCacheServiceTests()
	{
		_memoryCache = new MemoryCache(new MemoryCacheOptions());
		_sut = new MemoryCacheService(_memoryCache);
	}

	public void Dispose()
	{
		_memoryCache.Dispose();
	}

	[Fact]
	public void SetValue_ShouldStoreValueUnderPrefixedKey()
	{
		// Arrange
		const string key = "key";
		const string keyPrefix = "prefix";
		const string value = "value";

		// Act
		_sut.SetValue(key, value, keyPrefix);

		// Assert
		_memoryCache.TryGetValue($"{keyPrefix}-{key}", out string? storedValue).Should().BeTrue();
		storedValue.Should().Be(value);
	}

	[Fact]
	public void SetValue_ShouldStoreValueUnderPlainKey_WhenPrefixIsEmpty()
	{
		// Arrange
		const string key = "key";
		const string value = "value";

		// Act
		_sut.SetValue(key, value);

		// Assert
		_memoryCache.TryGetValue(key, out string? storedValue).Should().BeTrue();
		storedValue.Should().Be(value);
	}

	[Fact]
	public void SetValue_ShouldThrow_WhenValueIsNull()
	{
		// Arrange
		Action act = () => _sut.SetValue<string?>("key", null);

		// Act & Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void GetValue_ShouldReturnStoredValueAndRemoveItByDefault()
	{
		// Arrange
		const string key = "key";
		const string keyPrefix = "prefix";
		const string value = "value";
		_sut.SetValue(key, value, keyPrefix);

		// Act
		var result = _sut.GetValue<string>(key, keyPrefix);

		// Assert
		result.exists.Should().BeTrue();
		result.value.Should().Be(value);
		_memoryCache.TryGetValue($"{keyPrefix}-{key}", out _).Should().BeFalse();
	}

	[Fact]
	public void GetValue_ShouldKeepStoredValue_WhenRemoveIsFalse()
	{
		// Arrange
		const string key = "key";
		const string value = "value";
		_sut.SetValue(key, value);

		// Act
		var result = _sut.GetValue<string>(key, remove: false);

		// Assert
		result.exists.Should().BeTrue();
		result.value.Should().Be(value);
		_memoryCache.TryGetValue(key, out _).Should().BeTrue();
	}

	[Fact]
	public void GetValue_ShouldReturnMissing_WhenKeyWasNotStored()
	{
		// Act
		var result = _sut.GetValue<string>("missing-key");

		// Assert
		result.exists.Should().BeFalse();
		result.value.Should().BeNull();
	}

	[Fact]
	public void RemoveValue_ShouldRemoveStoredValue()
	{
		// Arrange
		const string key = "key";
		const string keyPrefix = "prefix";
		_sut.SetValue(key, "value", keyPrefix);

		// Act
		_sut.RemoveValue(key, keyPrefix);

		// Assert
		_memoryCache.TryGetValue($"{keyPrefix}-{key}", out _).Should().BeFalse();
	}

	[Fact]
	public void RemoveValue_ShouldNotThrow_WhenKeyWasNotStored()
	{
		// Act
		Action act = () => _sut.RemoveValue("missing-key");

		// Assert
		act.Should().NotThrow();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void SetValue_ShouldThrow_WhenKeyIsNullOrWhiteSpace(string? key)
	{
		// Arrange
		Action act = () => _sut.SetValue(key!, "value");

		// Act & Assert
		act.Should().Throw<ArgumentException>();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void GetValue_ShouldThrow_WhenKeyIsNullOrWhiteSpace(string? key)
	{
		// Arrange
		Action act = () => _sut.GetValue<string>(key!);

		// Act & Assert
		act.Should().Throw<ArgumentException>();
	}
}
