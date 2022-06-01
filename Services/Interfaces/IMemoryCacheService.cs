namespace SecretMessageSharingWebApp.Services.Interfaces
{
	public interface IMemoryCacheService
	{
		void SetValue(string key, string value);

		(bool exists, string value) GetValue(string key, bool remove = true);

		void RemoveValue(string key);
	}
}
