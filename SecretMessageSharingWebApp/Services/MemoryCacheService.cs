﻿using Microsoft.Extensions.Caching.Memory;
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

		public void SetValue<TItem>(string key, TItem value, string keyPrefix = "")
		{
			var expirationTimeSpan = TimeSpan.FromMinutes(Constants.MemoryCacheValueExpirationTimeInMinutes);

			var cacheEntryOptions = new MemoryCacheEntryOptions()
			{
				AbsoluteExpirationRelativeToNow = expirationTimeSpan
			};

			var cancellationTokenSource = new CancellationTokenSource(expirationTimeSpan);
			cacheEntryOptions.AddExpirationToken(new CancellationChangeToken(cancellationTokenSource.Token));

			var cacheKey = GetCacheKey(key, keyPrefix);
			_memoryCache.Set(cacheKey, value, cacheEntryOptions);
		}

		public (bool exists, TItem value) GetValue<TItem>(string key, string keyPrefix = "", bool remove = true)
		{
			var cacheKey = GetCacheKey(key, keyPrefix);

			var exists = _memoryCache.TryGetValue(cacheKey, out TItem value);
			if (exists && remove)
			{
				RemoveValue(key, keyPrefix);
			}

			return (exists, value);
		}

		public void RemoveValue(string key, string keyPrefix = "")
		{
			var cacheKey = GetCacheKey(key, keyPrefix);

			_memoryCache.Remove(cacheKey);
		}

		private string GetCacheKey(string key, string keyPrefix)
		{
			var cacheKey = (keyPrefix == string.Empty) ? key : $"{keyPrefix}-{key}";
			return cacheKey;
		}
	}
}
