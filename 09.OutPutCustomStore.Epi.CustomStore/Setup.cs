using EPiServer.ServiceLocation;
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
