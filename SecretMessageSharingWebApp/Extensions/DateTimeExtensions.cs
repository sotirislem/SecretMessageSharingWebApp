namespace SecretMessageSharingWebApp.Extensions
{
	public static class DateTimeExtensions
	{
		public static DateTime ToLocalTimeZone(this DateTime dateTime)
		{
			var destinationTimeZone = TimeZoneInfo.FindSystemTimeZoneById(Constants.LocalTimeZone);
			var convertedDateTime = TimeZoneInfo.ConvertTime(dateTime, destinationTimeZone);
			
			return convertedDateTime;
		}
	}
}
