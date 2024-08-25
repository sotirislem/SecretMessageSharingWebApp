using Microsoft.AspNetCore.Http;
using SecretMessageSharingWebApp.Models.Common;
using SecretMessageSharingWebApp.Services.Interfaces;
using SecretMessageSharingWebApp.Services;
using System.Text.Json;
using System.Text;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class RecentlyStoredMessagesServiceTests
{
	private readonly IFixture _fixture;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ISecretMessagesService _secretMessagesService;
	private readonly IGetLogsService _getLogsService;

	private readonly RecentlyStoredMessagesService _sut;


	public RecentlyStoredMessagesServiceTests()
	{
		_fixture = new Fixture();
		_httpContextAccessor = Substitute.For<IHttpContextAccessor>();
		_secretMessagesService = Substitute.For<ISecretMessagesService>();
		_getLogsService = Substitute.For<IGetLogsService>();

		_sut = new RecentlyStoredMessagesService(_httpContextAccessor, _secretMessagesService, _getLogsService);
	}

	[Fact]
	public async Task GetAll_ShouldReturnEmptyList_WhenNoMessagesInSession()
	{
		// Arrange
		var httpContext = Substitute.For<HttpContext>();
		var session = Substitute.For<ISession>();

		httpContext.Session = session;
		_httpContextAccessor.HttpContext = httpContext;

		session.TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]?>())
			.Returns(false);

		// Act
		var result = await _sut.GetAll();

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAll_ShouldReturnCombinedAndOrderedMessages_WhenSessionHasMessages()
	{
		// Arrange
		var httpContext = Substitute.For<HttpContext>();
		var session = Substitute.For<ISession>();

		httpContext.Session = session;
		_httpContextAccessor.HttpContext = httpContext;

		var recentMessagesList = _fixture.Create<List<string>>();
		var secretMessages = _fixture.CreateMany<RecentlyStoredSecretMessage>(2).ToList();
		var logMessages = _fixture.CreateMany<RecentlyStoredSecretMessage>(2).ToList();

		var serializedList = JsonSerializer.Serialize(recentMessagesList);
		var byteArray = Encoding.UTF8.GetBytes(serializedList);

		session.TryGetValue(Arg.Is(Constants.SessionKeys.RecentlyStoredSecretMessagesList), out Arg.Any<byte[]?>())
			.Returns(callInfo =>
			{
				callInfo[1] = byteArray; // Set out parameter
				return true;
			});

		_secretMessagesService.GetRecentlyStoredSecretMessagesInfo(Arg.Is<List<string>>(list => list.SequenceEqual(recentMessagesList)))
			.Returns(Task.FromResult(secretMessages));

		_getLogsService.GetRecentlyStoredSecretMessagesInfo(Arg.Is<List<string>>(list => list.SequenceEqual(recentMessagesList)))
			.Returns(Task.FromResult(logMessages));

		var expectedResult = secretMessages.Union(logMessages)
			.OrderByDescending(x => x.CreatedDateTime)
			.ToList();

		// Act
		var result = await _sut.GetAll();

		// Assert
		result.Should().BeEquivalentTo(expectedResult);
	}

	[Fact]
	public void GetRecentlyStoredSecretMessagesList_ShouldReturnEmptyList_WhenSessionHasNoData()
	{
		// Arrange
		var httpContext = Substitute.For<HttpContext>();
		var session = Substitute.For<ISession>();

		httpContext.Session = session;
		_httpContextAccessor.HttpContext = httpContext;

		session.TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]?>())
			.Returns(false);

		// Act
		var result = _sut.GetRecentlyStoredSecretMessagesList();

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetRecentlyStoredSecretMessagesList_ShouldReturnMessagesFromSession()
	{
		// Arrange
		var expectedList = _fixture.Create<List<string>>();

		var httpContext = Substitute.For<HttpContext>();
		var session = Substitute.For<ISession>();

		httpContext.Session = session;
		_httpContextAccessor.HttpContext = httpContext;

		var serializedList = JsonSerializer.Serialize(expectedList);
		var byteArray = Encoding.UTF8.GetBytes(serializedList);

		session.TryGetValue(Arg.Is(Constants.SessionKeys.RecentlyStoredSecretMessagesList), out Arg.Any<byte[]?>())
			.Returns(callInfo =>
			{
				callInfo[1] = byteArray; // Set out parameter
				return true;
			});

		// Act
		var result = _sut.GetRecentlyStoredSecretMessagesList();

		// Assert
		result.Should().BeEquivalentTo(expectedList);
	}
}