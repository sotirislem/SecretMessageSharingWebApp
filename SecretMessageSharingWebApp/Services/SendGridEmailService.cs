using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using SecretMessageSharingWebApp.ConfigurationSettings;
using SecretMessageSharingWebApp.Models.Api.Responses;
using SecretMessageSharingWebApp.Models.Domain;
using SecretMessageSharingWebApp.Providers;
using SecretMessageSharingWebApp.Services.Interfaces;
using SendGrid.Helpers.Mail;

namespace SecretMessageSharingWebApp.Services;

public sealed class SendGridEmailService(
	ILogger<SendGridEmailService> logger,
	ICancellationTokenProvider cancellationTokenProvider,
	SendGridConfigurationSettings sendGridConfigurationSettings,
	ISendGridClientFactory sendGridClientFactory) : ISendGridEmailService
{
	public async Task<bool> SendOtp(
		OneTimePassword otp,
		string messageId,
		string recipientsEmail,
		string? recipientTimeZone,
		int? recipientUtcOffsetMinutes)
	{
		if (Debugger.IsAttached)
		{
			Console.WriteLine($"OTP Code: {otp.Code}");
			return true;
		}

		var (subject, htmlContent) = CreateEmailContent(otp, messageId, recipientTimeZone, recipientUtcOffsetMinutes);

		return await SendEmailAsync("OTP", messageId, recipientsEmail, subject, htmlContent);
	}

	public async Task<bool> SendDeliveryNotification(SecretMessageDeliveryNotification notification, string recipientsEmail)
	{
		if (Debugger.IsAttached)
		{
			Console.WriteLine($"Delivery notification email requested for message: {notification.MessageId}");
			return true;
		}

		var (subject, htmlContent) = CreateDeliveryNotificationEmailContent(notification);

		return await SendEmailAsync("delivery notification", notification.MessageId, recipientsEmail, subject, htmlContent);
	}

	private async Task<bool> SendEmailAsync(
		string emailType,
		string messageId,
		string recipientsEmail,
		string subject,
		string htmlContent)
	{
		var cancellationToken = cancellationTokenProvider.Token;

		try
		{
			var client = sendGridClientFactory.Create(sendGridConfigurationSettings.ApiKey);
			var from = new EmailAddress(sendGridConfigurationSettings.AuthSender, Constants.AppName);
			var to = new EmailAddress(recipientsEmail);

			var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent);
			var response = await client.SendEmailAsync(msg, cancellationToken);

			if (response.IsSuccessStatusCode is false)
			{
				logger.LogError(
					"Failed to send {EmailType} email for message: {messageId} to: {recipientsEmail} | SendGrid API status code: {httpStatusCode} ({httpStatusCodeInt})",
					emailType, messageId, recipientsEmail, response.StatusCode, (int)response.StatusCode);
			}

			return response.IsSuccessStatusCode;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			logger.LogError(exception,
				"Failed to send {EmailType} email for message: {messageId} to: {recipientsEmail} after retrying transient failures",
				emailType, messageId, recipientsEmail);

			return false;
		}
	}

	private static (string subject, string htmlContent) CreateEmailContent(
		OneTimePassword otp,
		string messageId,
		string? recipientTimeZone,
		int? recipientUtcOffsetMinutes)
	{
		var subject = $"{Constants.AppName} - OTP";

		var bodyContent = EmailTemplateRenderer.Render(EmailTemplateNames.OtpBody, new Dictionary<string, string>
		{
			["Code"] = HtmlEncode(otp.Code),
			["ExpirationMinutes"] = Constants.OtpExpirationMinutes.ToString(CultureInfo.InvariantCulture),
			["ExpiresOn"] = FormatExpiration(otp.ExpiresAt, recipientTimeZone, recipientUtcOffsetMinutes)
		});

		var htmlContent = RenderLayout(
			badge: "Private",
			eyebrow: "One-time code",
			heading: "Your secure code",
			messageId: messageId,
			bodyContent: bodyContent,
			footerNote: "Secure one-time access");

		return (subject, htmlContent);
	}

	private static (string subject, string htmlContent) CreateDeliveryNotificationEmailContent(
		SecretMessageDeliveryNotification notification)
	{
		var subject = $"{Constants.AppName} - Message delivery details";

		var rows = new List<DetailRow>
		{
			new("Message created on", FormatDateTime(notification.MessageCreatedOn)),
			new("Message opened on", FormatDateTime(notification.MessageDeliveredOn))
		};

		var timeToOpen = FormatTimeToOpen(notification.MessageCreatedOn, notification.MessageDeliveredOn);
		if (timeToOpen is not null)
		{
			rows.Add(new("Time to open", timeToOpen));
		}

		rows.Add(new("Opened from", FormatLocation(notification), FormatLocationSource(notification)));
		rows.Add(new("Opened from IP", FormatValue(notification.RecipientIP)));
		rows.Add(new("Opened from device", FormatValue(notification.RecipientClientInfo)));

		var isp = FormatValue(notification.RecipientIsp);
		if (isp is not null)
		{
			rows.Add(new("Network provider", isp));
		}

		var timezone = FormatTimezone(notification);
		if (timezone is not null)
		{
			rows.Add(new("Recipient time zone", timezone));
		}

		var language = FormatLanguage(notification.RecipientLanguage);
		if (language is not null)
		{
			rows.Add(new("Browser language", language));
		}

		if (notification.OtpVerified)
		{
			rows.Add(new("OTP verified", "Yes", ValueStyle: "color:#0f9d76;font-weight:700;"));
		}

		var bodyContent = EmailTemplateRenderer.Render(
			EmailTemplateNames.DeliveryNotificationBody,
			new Dictionary<string, string>
			{
				["DetailRows"] = BuildDetailRows(rows)
			});

		var htmlContent = RenderLayout(
			badge: "Notification",
			eyebrow: "Delivery update",
			heading: "Your message was opened",
			messageId: notification.MessageId,
			bodyContent: bodyContent,
			footerNote: "Delivery notification");

		return (subject, htmlContent);
	}

	private static string RenderLayout(
		string badge,
		string eyebrow,
		string heading,
		string messageId,
		string bodyContent,
		string footerNote)
	{
		var appName = HtmlEncode(Constants.AppName);

		return EmailTemplateRenderer.Render(EmailTemplateNames.Layout, new Dictionary<string, string>
		{
			["AppName"] = appName,
			["AppUrl"] = Constants.AppUrl,
			["Badge"] = HtmlEncode(badge),
			["Eyebrow"] = HtmlEncode(eyebrow),
			["Heading"] = HtmlEncode(heading),
			["MessageId"] = HtmlEncode(messageId),
			["Body"] = bodyContent,
			["FooterNote"] = HtmlEncode(footerNote)
		});
	}

	private static string BuildDetailRows(IReadOnlyList<DetailRow> rows)
	{
		var builder = new StringBuilder();

		for (var index = 0; index < rows.Count; index++)
		{
			if (builder.Length > 0)
			{
				builder.AppendLine();
			}

			builder.Append(RenderDetailRow(rows[index], index, isLastRow: index == rows.Count - 1));
		}

		return builder.ToString();
	}

	private static string RenderDetailRow(DetailRow row, int index, bool isLastRow)
	{
		var background = index % 2 == 0 ? "#f4f2fc" : "#fbfbfe";
		var bottomBorder = isLastRow ? string.Empty : "border-bottom:1px solid #eef0f6;";
		var hint = string.IsNullOrWhiteSpace(row.Hint)
			? string.Empty
			: EmailTemplateRenderer.Render(EmailTemplateNames.DeliveryDetailRowHint, new Dictionary<string, string>
			{
				["Hint"] = HtmlEncode(row.Hint)
			});

		return EmailTemplateRenderer.Render(EmailTemplateNames.DeliveryDetailRow, new Dictionary<string, string>
		{
			["Background"] = background,
			["LabelStyle"] = $"background-color:{background};padding:11px 14px;{bottomBorder}font-size:12px;line-height:1.4;color:#7b8195;",
			["Label"] = HtmlEncode(row.Label),
			["ValueStyle"] = $"background-color:{background};padding:11px 14px;{bottomBorder}font-size:12px;line-height:1.4;{row.ValueStyle ?? "color:#171a2b;font-weight:600;"}word-break:break-word;",
			["Value"] = HtmlEncode(row.Value ?? "Not available"),
			["Hint"] = hint
		});
	}

	private static string FormatExpiration(
		DateTimeOffset expiresAt,
		string? recipientTimeZone,
		int? recipientUtcOffsetMinutes)
	{
		var utcExpiration = expiresAt.ToUniversalTime();

		if (string.IsNullOrWhiteSpace(recipientTimeZone) is false
		    && TimeZoneInfo.TryFindSystemTimeZoneById(recipientTimeZone, out var timeZone))
		{
			var localExpiration = TimeZoneInfo.ConvertTime(utcExpiration, timeZone);

			return $"{FormatDateTime(localExpiration)} ({HtmlEncode(recipientTimeZone)}, {FormatUtcOffset((int)localExpiration.Offset.TotalMinutes)})";
		}

		if (recipientUtcOffsetMinutes is not null)
		{
			var localExpiration = utcExpiration.ToOffset(TimeSpan.FromMinutes(recipientUtcOffsetMinutes.Value));

			return $"{FormatDateTime(localExpiration)} ({FormatUtcOffset(recipientUtcOffsetMinutes.Value)})";
		}

		return $"{FormatDateTime(utcExpiration)} UTC";
	}

	private static string FormatDateTime(DateTimeOffset dateTime)
	{
		return dateTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
	}

	private static string FormatDateTime(DateTime dateTime)
	{
		return dateTime.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
	}

	private static string? FormatTimeToOpen(DateTime createdOn, DateTime deliveredOn)
	{
		if (deliveredOn < createdOn)
		{
			return null;
		}

		var durationMs = (deliveredOn - createdOn).TotalMilliseconds;
		var totalSeconds = (long)Math.Round(durationMs / 1000);

		if (totalSeconds < 60)
		{
			return "Less than a minute";
		}

		var totalMinutes = totalSeconds / 60;
		if (totalMinutes < 60)
		{
			return Pluralize(totalMinutes, "minute");
		}

		var totalHours = totalMinutes / 60;
		if (totalHours < 24)
		{
			var minutes = totalMinutes % 60;

			return JoinParts(
				Pluralize(totalHours, "hour"),
				minutes > 0 ? Pluralize(minutes, "minute") : null);
		}

		var days = totalHours / 24;
		var hours = totalHours % 24;

		return JoinParts(
			Pluralize(days, "day"),
			hours > 0 ? Pluralize(hours, "hour") : null);
	}

	private static string FormatLocation(SecretMessageDeliveryNotification notification)
	{
		var place = string.Join(
			", ",
			new[] { notification.RecipientCity, notification.RecipientRegion, notification.RecipientCountry }
				.Where(part => string.IsNullOrWhiteSpace(part) is false)
				.Select(part => part!.Trim()));

		if (string.IsNullOrWhiteSpace(place))
		{
			return FormatLocationSource(notification) is null
				? "Not available"
				: "Unknown";
		}

		var flag = GetCountryFlag(notification.RecipientCountryCode);

		return flag is null ? place : $"{flag} {place}";
	}

	private static string? FormatLocationSource(SecretMessageDeliveryNotification notification)
	{
		return notification.RecipientLocationSource switch
		{
			"device" => "Precise (device location)",
			"ip" => "Approximate (IP address)",
			_ => null
		};
	}

	private static string? FormatTimezone(SecretMessageDeliveryNotification notification)
	{
		var timezone = FormatValue(notification.RecipientTimezone);

		if (notification.RecipientUtcOffsetMinutes is null)
		{
			return timezone;
		}

		var offset = FormatUtcOffset(notification.RecipientUtcOffsetMinutes.Value);

		return timezone is null ? offset : $"{timezone} ({offset})";
	}

	private static string? FormatLanguage(string? language)
	{
		var value = FormatValue(language);
		if (value is null)
		{
			return null;
		}

		try
		{
			var culture = CultureInfo.GetCultureInfo(value);
			var languageCulture = culture.IsNeutralCulture ? culture : culture.Parent;
			var displayName = FormatValue(languageCulture.NativeName);

			return displayName is null || string.Equals(displayName, value, StringComparison.OrdinalIgnoreCase)
				? value
				: $"{displayName} ({value})";
		}
		catch (CultureNotFoundException)
		{
			return value;
		}
	}

	private static string FormatUtcOffset(int offsetMinutes)
	{
		var sign = offsetMinutes < 0 ? '-' : '+';
		var absolute = Math.Abs(offsetMinutes);
		var hours = absolute / 60;
		var minutes = absolute % 60;

		return $"UTC{sign}{hours}:{minutes:D2}";
	}

	private static string? GetCountryFlag(string? countryCode)
	{
		if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2 || countryCode.All(char.IsLetter) is false)
		{
			return null;
		}

		var codePoints = countryCode
			.ToUpperInvariant()
			.Select(character => 0x1F1E6 + character - 'A')
			.ToArray();

		return string.Concat(codePoints.Select(codePoint => char.ConvertFromUtf32(codePoint)));
	}

	private static string? FormatValue(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
	}

	private static string HtmlEncode(string value)
	{
		return WebUtility.HtmlEncode(value);
	}

	private static string Pluralize(long value, string unit)
	{
		return $"{value} {unit}{(value == 1 ? string.Empty : "s")}";
	}

	private static string JoinParts(params string?[] parts)
	{
		return string.Join(" ", parts.Where(part => string.IsNullOrWhiteSpace(part) is false));
	}

	private sealed record DetailRow(string Label, string? Value, string? Hint = null, string? ValueStyle = null);
}
