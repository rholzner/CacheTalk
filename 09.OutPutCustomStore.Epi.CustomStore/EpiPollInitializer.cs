using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Microsoft.AspNetCore.OutputCaching;

namespace _09.OutPutCustomStore.Epi.CustomStore;

[ModuleDependency(typeof(InitializationModule))]
public class EpiPollInitializer : IInitializableModule
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
            AsyncUtil.RunSync(() => outputCacheStore.EvictByTagAsync("epi", default).AsTask());
            AsyncUtil.RunSync(() => outputCacheStore.EvictByTagAsync(content.ContentLink.ID.ToString(), default).AsTask());
        }
    }

    public void Uninitialize(InitializationEngine context)
    {
    }
}
