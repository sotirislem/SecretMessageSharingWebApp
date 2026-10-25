using SecretMessageSharingWebApp.Extensions;

namespace SecretMessageSharingWebApp.UnitTests.ExtensionsTests;

public class StringExtensionsTests
{
	[Theory]
	[InlineData("192.168.1.100", "192.168.1.xxx")]
	[InlineData("8.8.8.8", "8.8.8.xxx")]
	public void MaskIpAddress_ShouldMaskLastIpv4Octet(string ipAddress, string expected)
	{
		// Act
		var result = ipAddress.MaskIpAddress();

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void MaskIpAddress_ShouldMaskIpv4MappedIpv6AddressAsIpv4()
	{
		// Act
		var result = "::ffff:192.168.1.100".MaskIpAddress();

		// Assert
		result.Should().Be("192.168.1.xxx");
	}

	[Theory]
	[InlineData("fe80::f2de:f1ff:fe41:dc92", "fe80::/64")]
	[InlineData("2001:4860:4860::8888", "2001:4860:4860::/64")]
	public void MaskIpAddress_ShouldKeepOnlyIpv6NetworkPrefix(string ipAddress, string expected)
	{
		// Act
		var result = ipAddress.MaskIpAddress();

		// Assert
		result.Should().Be(expected);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void MaskIpAddress_ShouldReturnNullOrEmptyForMissingValue(string? ipAddress)
	{
		// Act
		var result = ipAddress.MaskIpAddress();

		// Assert
		result.Should().BeNull();
	}

	[Theory]
	[InlineData("localhost")]
	[InlineData("Unknown")]
	public void MaskIpAddress_ShouldReturnNonIpValuesUnchanged(string value)
	{
		// Act
		var result = value.MaskIpAddress();

		// Assert
		result.Should().Be(value);
	}
}
