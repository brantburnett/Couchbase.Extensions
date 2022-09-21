using Couchbase.Extensions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Extensions.Session.UnitTests
{
    /// <summary>
    /// A "fake" cache for unit testing Couchbase caching.
    /// </summary>
    internal class CouchbaseInMemoryCache : ICouchbaseCache
    {
        private readonly IDistributedCache _cache;

        public CouchbaseInMemoryCache(IDistributedCache cache)
        {
            _cache = cache;
        }
        public bool DisableGet { get; set; }
        public bool DisableSetAsync { get; set; }
        public bool DisableRefreshAsync { get; set; }
        public bool DelayGetAsync { get; set; }
        public bool DelaySetAsync { get; set; }
        public bool DelayRefreshAsync { get; set; }
        public ICouchbaseCacheCollectionProvider CollectionProvider { get; }

        public CouchbaseCacheOptions Options { get; }

        public byte[] Get(string key)
        {
            if (DisableGet)
            {
                throw new InvalidOperationException();
            }
            return _cache.Get(key);
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (DisableGet)
            {
                throw new InvalidOperationException();
            }
            if (DelayGetAsync)
            {
                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                token.ThrowIfCancellationRequested();
            }
            return _cache.GetAsync(key, token);
        }

        public TimeSpan GetLifetime(DistributedCacheEntryOptions options = null)
        {
            return CouchbaseCacheExtensions.GetLifetime(this, options);
        }

        public void Refresh(string key)
        {
            _cache.Refresh(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (DisableRefreshAsync)
            {
                throw new InvalidOperationException();
            }
            if (DelayRefreshAsync)
            {
                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                token.ThrowIfCancellationRequested();
            }
            return _cache.RefreshAsync(key, token);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return _cache.RemoveAsync(key, token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _cache.Set(key, value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (DisableSetAsync)
            {
                throw new InvalidOperationException();
            }
            if (DelaySetAsync)
            {
                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                token.ThrowIfCancellationRequested();
            }
            return _cache.SetAsync(key, value, options, token);
        }
    }
}
