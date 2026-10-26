using System.Collections.Concurrent;
using System.Text;

namespace SecretMessageSharingWebApp.Services;

/// <summary>
/// Renders the HTML emails from the files in the <c>EmailTemplates</c> folder, which are
/// embedded into the assembly so the published app always carries them.
/// A template contains <c>{{Token}}</c> placeholders; every placeholder must be supplied
/// by the caller, so a missing value fails loudly instead of reaching a recipient.
/// </summary>
public static class EmailTemplateRenderer
{
	private const string TemplateFolder = "EmailTemplates";
	private const string TokenStart = "{{";
	private const string TokenEnd = "}}";

	private static readonly ConcurrentDictionary<string, string> Templates =
		new(StringComparer.OrdinalIgnoreCase);

	public static string Render(string templateName, IReadOnlyDictionary<string, string> tokens)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
		ArgumentNullException.ThrowIfNull(tokens);

		var rendered = Templates.GetOrAdd(templateName, LoadTemplate);

		foreach (var (token, value) in tokens)
		{
			rendered = rendered.Replace($"{TokenStart}{token}{TokenEnd}", value, StringComparison.Ordinal);
		}

		var unresolvedToken = FindToken(rendered);
		if (unresolvedToken is not null)
		{
			throw new InvalidOperationException(
				$"The email template '{templateName}' contains the unresolved token '{unresolvedToken}'.");
		}

		return rendered;
	}

	private static string LoadTemplate(string templateName)
	{
		var assembly = typeof(EmailTemplateRenderer).Assembly;
		var resourceSuffix = $".{TemplateFolder}.{templateName}.html";
		var resourceName = assembly.GetManifestResourceNames()
			.FirstOrDefault(name => name.EndsWith(resourceSuffix, StringComparison.Ordinal));

		if (resourceName is null)
		{
			throw new FileNotFoundException(
				$"The embedded email template '{templateName}.html' was not found in '{assembly.GetName().Name}'.");
		}

		using var stream = assembly.GetManifestResourceStream(resourceName)!;
		using var reader = new StreamReader(stream, Encoding.UTF8);

		return reader.ReadToEnd();
	}

	private static string? FindToken(string text)
	{
		var start = text.IndexOf(TokenStart, StringComparison.Ordinal);
		if (start < 0)
		{
			return null;
		}

		var end = text.IndexOf(TokenEnd, start, StringComparison.Ordinal);

		return end < 0
			? text[start..]
			: text[start..(end + TokenEnd.Length)];
	}
}
