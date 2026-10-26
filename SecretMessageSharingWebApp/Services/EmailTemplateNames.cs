namespace SecretMessageSharingWebApp.Services;

/// <summary>
/// File names (without extension) of the embedded HTML email templates.
/// They map to the <c>EmailTemplates</c> folder.
/// </summary>
public static class EmailTemplateNames
{
	public const string Layout = "EmailLayout";
	public const string OtpBody = "OtpEmailBody";
	public const string DeliveryNotificationBody = "DeliveryNotificationEmailBody";
	public const string DeliveryDetailRow = "DeliveryDetailRow";
	public const string DeliveryDetailRowHint = "DeliveryDetailRowHint";
}
