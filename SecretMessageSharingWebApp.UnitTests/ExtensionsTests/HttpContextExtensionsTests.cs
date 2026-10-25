using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SecretMessageSharingWebApp.Extensions;

namespace SecretMessageSharingWebApp.UnitTests.ExtensionsTests;

public class HttpContextExtensionsTests
{
	[Theory]
	[InlineData("::1")]
	[InlineData("127.0.0.1")]
	public void GetClientIP_ShouldReturnLocalhostForLoopbackRequestsInDevelopment(string loopbackAddress)
	{
		// Arrange
		var context = CreateHttpContext(IPAddress.Parse(loopbackAddress), Environments.Development);

		// Act
		var result = context.GetClientIP();

		// Assert
		result.Should().Be("localhost");
	}

	[Fact]
	public void GetClientIP_ShouldKeepLoopbackAddressOutsideDevelopment()
	{
		// Arrange
		var context = CreateHttpContext(IPAddress.IPv6Loopback, Environments.Production);

		// Act
		var result = context.GetClientIP();

		// Assert
		result.Should().Be("::1");
	}

	[Fact]
	public void GetClientIP_ShouldReturnRemoteAddressForNonLoopbackRequests()
	{
		// Arrange
		var context = CreateHttpContext(IPAddress.Parse("203.0.113.10"), Environments.Development);

		// Act
		var result = context.GetClientIP();

		// Assert
		result.Should().Be("203.0.113.10");
	}

	[Fact]
	public void GetClientIP_ShouldReturnUnknownWhenRemoteAddressIsMissing()
	{
		// Arrange
		var context = new DefaultHttpContext();

		// Act
		var result = context.GetClientIP();

		// Assert
		result.Should().Be("Unknown");
	}

	private static DefaultHttpContext CreateHttpContext(IPAddress remoteIpAddress, string environmentName)
	{
		var environment = Substitute.For<IWebHostEnvironment>();
		environment.EnvironmentName.Returns(environmentName);

		var services = new ServiceCollection()
			.AddSingleton(environment)
			.BuildServiceProvider();

		return new DefaultHttpContext
		{
			RequestServices = services,
			Connection = { RemoteIpAddress = remoteIpAddress }
		};
	}
}
