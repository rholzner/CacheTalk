using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddOutputCache();

builder.Services.RemoveAll<IOutputCacheStore>();
builder.Services.AddSingleton<IOutputCacheStore, CustomOutputCacheStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseOutputCache();

app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-7.0#cache-storage
/// </summary>
public class CustomOutputCacheStore : IOutputCacheStore
{
    private readonly IMemoryCache memoryCache;

    public CustomOutputCacheStore(IMemoryCache memoryCache)
    {
        this.memoryCache = memoryCache;
    }
    public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        memoryCache.Remove(tag);
        return ValueTask.CompletedTask;
    }

    public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
    {
        memoryCache.TryGetValue(key, out byte[]? value);
        return ValueTask.FromResult(value);
    }

    public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
    {
        memoryCache.Set(key, value, validFor);
        return ValueTask.CompletedTask;
    }
}