using Microsoft.EntityFrameworkCore;
using SecretMessageSharingWebApp.Data;
using SecretMessageSharingWebApp.Data.Entities;

namespace SecretMessageSharingWebApp.UnitTests.DataTests;

public class SecretMessagesDbContextTests
{
	[Fact]
	public void Model_ShouldConfigureEtagConcurrencyForSecretMessages()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<SecretMessagesDbContext>()
			.UseCosmos(
				accountEndpoint: "https://localhost:8081",
				accountKey: Convert.ToBase64String(new byte[64]),
				databaseName: "unit-tests")
			.Options;

		// Act
		using var context = new SecretMessagesDbContext(options);
		var entityType = context.Model.FindEntityType(typeof(SecretMessageEntity));

		// Assert
		// Building the model also runs the Cosmos model validator, which rejects
		// concurrency tokens that are not mapped to the Cosmos "_etag" property.
		entityType.Should().NotBeNull();
		entityType!.GetETagProperty().Should().NotBeNull();
	}
}
