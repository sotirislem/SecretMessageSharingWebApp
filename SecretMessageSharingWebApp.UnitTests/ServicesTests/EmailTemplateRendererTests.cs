using SecretMessageSharingWebApp.Services;

namespace SecretMessageSharingWebApp.UnitTests.ServicesTests;

public class EmailTemplateRendererTests
{
	[Theory]
	[InlineData(EmailTemplateNames.Layout)]
	[InlineData(EmailTemplateNames.OtpBody)]
	[InlineData(EmailTemplateNames.DeliveryNotificationBody)]
	[InlineData(EmailTemplateNames.DeliveryDetailRow)]
	[InlineData(EmailTemplateNames.DeliveryDetailRowHint)]
	public void Render_ShouldResolveEveryEmbeddedTemplate(string templateName)
	{
		// Act
		var render = () => EmailTemplateRenderer.Render(templateName, new Dictionary<string, string>());

		// Assert
		// An unreadable template surfaces as FileNotFoundException, so reaching the
		// unresolved token error proves the file was found and loaded.
		render.Should().Throw<InvalidOperationException>()
			.WithMessage("*unresolved token*");
	}

	[Fact]
	public void Render_ShouldReplaceEveryOccurrenceOfAToken()
	{
		// Arrange
		var tokens = new Dictionary<string, string>
		{
			["Heading"] = "Heading & more",
			["AppName"] = "TestApp",
			["AppUrl"] = "https://example.com/",
			["Badge"] = "Badge",
			["Eyebrow"] = "Eyebrow",
			["MessageId"] = "message-id",
			["Body"] = "<p>Body</p>",
			["FooterNote"] = "Footer"
		};

		// Act
		var result = EmailTemplateRenderer.Render(EmailTemplateNames.Layout, tokens);

		// Assert
		result.Should().Contain("<title>Heading & more</title>")
			.And.Contain(">TestApp</td>")
			.And.Contain(">TestApp</a>")
			.And.Contain("href=\"https://example.com/\"")
			.And.NotContain("{{");
	}

	[Fact]
	public void Render_ShouldThrow_WhenATokenHasNoValue()
	{
		// Arrange
		var tokens = new Dictionary<string, string>
		{
			["Heading"] = "Heading"
		};

		// Act
		var render = () => EmailTemplateRenderer.Render(EmailTemplateNames.Layout, tokens);

		// Assert
		render.Should().Throw<InvalidOperationException>()
			.WithMessage("*unresolved token '{{AppName}}'*");
	}

	[Fact]
	public void Render_ShouldThrow_WhenTheTemplateDoesNotExist()
	{
		// Act
		var render = () => EmailTemplateRenderer.Render("UnknownTemplate", new Dictionary<string, string>());

		// Assert
		render.Should().Throw<FileNotFoundException>()
			.WithMessage("*UnknownTemplate.html*");
	}
}
