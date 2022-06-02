using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using SecretMessageSharingWebApp.Services.Interfaces;

namespace SecretMessageSharingWebApp.Services
{
	public class MemoryCacheService : IMemoryCacheService
	{
		private readonly IMemoryCache _memoryCache;

		public MemoryCacheService(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public void SetValue(string key, string value)
		{
			var cacheEntryOptions = new MemoryCacheEntryOptions()
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.MemoryCacheValueExpirationTimeInMinutes)
			};

			var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(Constants.MemoryCacheValueExpirationTimeInMinutes));
			cacheEntryOptions.AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));

			_memoryCache.Set(key, value, cacheEntryOptions);
		}

		public (bool exists, string value) GetValue(string key, bool remove = true)
		{
			var exists = _memoryCache.TryGetValue(key, out string value);
			if (exists && remove)
			{
				RemoveValue(key);
			}

			return (exists, value);
		}

		public void RemoveValue(string key)
		{
			_memoryCache.Remove(key);
		}
	}
}
