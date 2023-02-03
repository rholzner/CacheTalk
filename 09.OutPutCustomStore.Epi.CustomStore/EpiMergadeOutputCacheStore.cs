using EPiServer.Framework.Cache;
using Microsoft.AspNetCore.OutputCaching;

namespace _09.OutPutCustomStore.Epi.CustomStore;

public class EpiMergadeOutputCacheStore : IOutputCacheStore
{
    private readonly ISynchronizedObjectInstanceCache cache;

    public EpiMergadeOutputCacheStore(ISynchronizedObjectInstanceCache cache)
    {
        this.cache = cache;
    }
    public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        cache.Remove(tag);
        return ValueTask.CompletedTask;
    }

    public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
    {
        cache.TryGet(key, ReadStrategy.Wait, out byte[]? value);
        return ValueTask.FromResult(value);
    }

    public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
    {
        if (tags is not null && tags.Any())
        {
            cache.Insert(key, value, new CacheEvictionPolicy(validFor, CacheTimeoutType.Absolute, tags));
            return ValueTask.CompletedTask;
        }

        cache.Insert(key, value, new CacheEvictionPolicy(validFor, CacheTimeoutType.Absolute));
        return ValueTask.CompletedTask;
    }
}
