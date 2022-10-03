using System.Text.Json;

namespace SecretMessageSharingWebApp.Extensions;

public static class SessionExtensions
{
	public static void SetObject(this ISession session, string key, object value)
	{
		session.SetString(key, JsonSerializer.Serialize(value));
	}

	public static T? GetObject<T>(this ISession session, string key)
	{
		var value = session.GetString(key);
		if (value is not null)
		{
			try
			{
				return JsonSerializer.Deserialize<T>(value);
			}
			catch
			{
				return default;
			}
		}

		return default;
	}
}
