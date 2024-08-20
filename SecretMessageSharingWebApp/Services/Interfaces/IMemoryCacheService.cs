namespace SecretMessageSharingWebApp.Services.Interfaces;

public interface IMemoryCacheService
{
	void SetValue<TItem>(string key, TItem value, string keyPrefix = "");

	(bool exists, TItem? value) GetValue<TItem>(string key, string keyPrefix = "", bool remove = true);

	void RemoveValue(string key, string keyPrefix = "");
}
