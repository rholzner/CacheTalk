using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace _09.OutPutCustomStore.Epi.CustomStore;

public static class Setup
{
    public static IServiceCollection UseEpiAsOutputCacheStore(this IServiceCollection services)
    {
        services.RemoveAll<IOutputCacheStore>();
        services.AddSingleton<IOutputCacheStore, EpiMergadeOutputCacheStore>();
        return services;
    }

    public static void AddEpiPoll(this OutputCacheOptions outputCacheOptions)
    {
        outputCacheOptions.AddPolicy("EpiTest", new EpiOutputCachePolicy());
    }
}

public class EpiOutputCachePolicy : IOutputCachePolicy
{

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var pageRouteHelper = context.HttpContext.RequestServices.GetService<IPageRouteHelper>();
        if (ContentReference.IsNullOrEmpty(pageRouteHelper?.ContentLink))
        {
            return ValueTask.CompletedTask;
        }

        context.Tags.Add(pageRouteHelper.ContentLink.ID.ToString());

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        return ValueTask.CompletedTask;
    }
}

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


[ModuleDependency(typeof(InitializationModule))]
public class OpenGraphImageInitializer : IInitializableModule
{
    public void Initialize(InitializationEngine context)
    {
        var contentEvents = context.Locate.ContentEvents();

        contentEvents.PublishingContent += ContentEvents_PublishingContent;
    }

    private void ContentEvents_PublishingContent(object sender, ContentEventArgs e)
    {
        var outputCacheStore = ServiceLocator.Current.GetInstance<IOutputCacheStore>();

        if (e.Content is IContent content)
        {
            AsyncUtil.RunSync(() => outputCacheStore.EvictByTagAsync(content.ContentLink.ID.ToString(), default).AsTask());
        }
    }

    public void Uninitialize(InitializationEngine context)
    {
    }
}

internal static class AsyncUtil
{
    private static readonly TaskFactory _taskFactory = new
        TaskFactory(CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default);

    /// <summary>
    /// Executes an async Task method which has a void return value synchronously
    /// USAGE: AsyncUtil.RunSync(() => AsyncMethod());
    /// </summary>
    /// <param name="task">Task method to execute</param>
    public static void RunSync(Func<Task> task)
        => _taskFactory
            .StartNew(task)
            .Unwrap()
            .GetAwaiter()
            .GetResult();

    /// <summary>
    /// Executes an async Task<T> method which has a T return type synchronously
    /// USAGE: T result = AsyncUtil.RunSync(() => AsyncMethod<T>());
    /// </summary>
    /// <typeparam name="TResult">Return Type</typeparam>
    /// <param name="task">Task<T> method to execute</param>
    /// <returns></returns>
    public static TResult RunSync<TResult>(Func<Task<TResult>> task)
        => _taskFactory
            .StartNew(task)
            .Unwrap()
            .GetAwaiter()
            .GetResult();
}