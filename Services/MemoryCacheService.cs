using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace SecretMessageSharingWebApp.Services
{
	public class MemoryCacheService
	{
		private const int ValueExpirationTimeInMinutes = 10;
		private readonly IMemoryCache _memoryCache;

		public MemoryCacheService(IMemoryCache memoryCache)
		{
			this._memoryCache = memoryCache;
		}

		public void SetValue(string key, string value)
		{
			var cacheEntryOptions = new MemoryCacheEntryOptions()
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ValueExpirationTimeInMinutes)
			};

			var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(ValueExpirationTimeInMinutes));
			cacheEntryOptions.AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));

			_memoryCache.Set(key, value, cacheEntryOptions);
		}

		public (bool exists, string value) GetValue(string key, bool remove = true)
		{
			var exists = _memoryCache.TryGetValue(key, out string value);
			if (exists && remove)
			{
				_memoryCache.Remove(key);
			}

			return (exists, value);
		}

		public void RemoveValue(string key)
		{
			_memoryCache.Remove(key);
		}
	}
}
