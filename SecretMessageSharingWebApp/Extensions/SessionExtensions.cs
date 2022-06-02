using Newtonsoft.Json;

namespace SecretMessageSharingWebApp.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (value is not null)
			{
                return JsonConvert.DeserializeObject<T>(value);

            }

            return default;
        }
    }
}
