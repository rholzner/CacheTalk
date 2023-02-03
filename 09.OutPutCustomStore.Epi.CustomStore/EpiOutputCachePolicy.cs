using EPiServer.Core;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;

namespace _09.OutPutCustomStore.Epi.CustomStore;

public class EpiOutputCachePolicy : IOutputCachePolicy
{

    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var pageRouteHelper = context.HttpContext.RequestServices.GetService<IPageRouteHelper>();
        if (ContentReference.IsNullOrEmpty(pageRouteHelper?.ContentLink))
        {
            return ValueTask.CompletedTask;
        }

        context.Tags.Add("epi");
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
