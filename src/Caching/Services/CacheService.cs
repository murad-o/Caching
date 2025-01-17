using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Caching.Abstractions.Entities;
using Caching.Abstractions.Key;
using Caching.Abstractions.Services;
using Caching.Exceptions;
using Caching.Helpers;
using Caching.Models;
using Caching.Serialization;
using Caching.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Caching.Services;

public class CacheService(
    IDistributedCache cache,
    IServiceProvider serviceProvider,
    ILogger<CacheService> logger,
    IOptions<CacheSettings> cacheSettings) : ICacheService
{
    private readonly IDistributedCache _cache = cache;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<CacheService> _logger = logger;
    private readonly LoggingSettings _loggingSettings = cacheSettings.Value.Logging;

    private readonly ResiliencePipeline _resiliencePipeline =
        serviceProvider.GetRequiredKeyedService<ResiliencePipeline>(ResiliencePipelines.CircuitBreakerPipeline);

    public Task<T> GetAsync<T>(string id, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, id);

        return GetByKeyAsync(key, entityOptions, cancellationToken);
    }

    public Task<T> GetByKeyAsync<T>(string key, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        return GetByKeyAsync(key, entityOptions, cancellationToken);
    }

    public Task SetAsync<T>(T value, CancellationToken cancellationToken) where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, value.GetId());
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);

        return SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task SetAsync<T>(T value, TimeSpan expirationTimeRelativeToNow, CancellationToken cancellationToken)
        where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, value.GetId());

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationTimeRelativeToNow);

        return SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task SetAsync<T>(T value, DateTime expirationDateTime, CancellationToken cancellationToken)
        where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, value.GetId());

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationDateTime);

        return SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task SetSlidingAsync<T>(T value, TimeSpan expirationTimeRelativeToNow, CancellationToken cancellationToken)
        where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, value.GetId());

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetSlidingExpiration(expirationTimeRelativeToNow);

        return SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task SetByKeyAsync<T>(string key, T value, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);

        return SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task SetByKeyAsync<T>(string key, T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationTimeRelativeToNow);

        return SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task SetByKeyAsync<T>(string key, T value, DateTime expirationDateTime, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationDateTime);

        return SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task SetSlidingByKeyAsync<T>(string key, T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetSlidingExpiration(expirationTimeRelativeToNow);

        return SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetAsync<T>(T value, CancellationToken cancellationToken) where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        var key = GetKey(entityOptions, value.GetId());

        return GetOrSetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetAsync<T>(T value, TimeSpan expirationTimeRelativeToNow, CancellationToken cancellationToken)
        where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, value.GetId());

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationTimeRelativeToNow);

        return GetOrSetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetAsync<T>(T value, DateTime expirationDateTime, CancellationToken cancellationToken)
        where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, value.GetId());

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationDateTime);

        return GetOrSetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetSlidingAsync<T>(T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken) where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, value.GetId());

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetSlidingExpiration(expirationTimeRelativeToNow);

        return GetOrSetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetByKeyAsync<T>(string key, T value, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);

        return GetOrSetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetByKeyAsync<T>(string key, T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationTimeRelativeToNow);

        return GetOrSetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetByKeyAsync<T>(string key, T value, DateTime expirationDateTime,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationDateTime);

        return GetOrSetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetSlidingByKeyAsync<T>(string key, T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetSlidingExpiration(expirationTimeRelativeToNow);

        return GetOrSetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetAsync<T>(string id, Func<Task<T>> valueFactory, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        var key = GetKey(entityOptions, id);

        return GetOrSetByKeyAsync(key, valueFactory, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetAsync<T>(string id, Func<Task<T>> valueFactory, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, id);

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationTimeRelativeToNow);

        return GetOrSetByKeyAsync(key, valueFactory, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetAsync<T>(string id, Func<Task<T>> valueFactory, DateTime expirationDateTime,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, id);

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationDateTime);

        return GetOrSetByKeyAsync(key, valueFactory, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetSlidingAsync<T>(string id, Func<Task<T>> valueFactory, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, id);

        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetSlidingExpiration(expirationTimeRelativeToNow);

        return GetOrSetByKeyAsync(key, valueFactory, cacheOptions, entityOptions, cancellationToken);
    }

    public async Task<T> GetOrSetByKeyAsync<T>(string key, Func<Task<T>> valueFactory,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);

        return await GetOrSetByKeyAsync(key, valueFactory, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetByKeyAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationTimeRelativeToNow);

        return GetOrSetByKeyAsync(key, valueFactory, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetByKeyAsync<T>(string key, Func<Task<T>> valueFactory, DateTime expirationDateTime,
        CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetAbsoluteExpiration(expirationDateTime);

        return GetOrSetByKeyAsync(key, valueFactory, cacheOptions, entityOptions, cancellationToken);
    }

    public Task<T> GetOrSetSlidingByKeyAsync<T>(string key, Func<Task<T>> valueFactory,
        TimeSpan expirationTimeRelativeToNow, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var cacheOptions = DistributedCacheEntryOptionsHelper.GetCacheOptions(entityOptions);
        cacheOptions.SetSlidingExpiration(expirationTimeRelativeToNow);

        return GetOrSetByKeyAsync(key, valueFactory, cacheOptions, entityOptions, cancellationToken);
    }

    public Task RemoveAsync<T>(T value, CancellationToken cancellationToken) where T : ICacheId
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, value.GetId());

        return RemoveByKeyAsync(key, entityOptions, cancellationToken);
    }

    public Task RemoveAsync<T>(string id, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        var key = GetKey(entityOptions, id);

        return RemoveByKeyAsync(key, entityOptions, cancellationToken);
    }

    public Task RemoveByKeyAsync<T>(string key, CancellationToken cancellationToken)
    {
        var entityOptions = GetEntityOptions<T>();
        return RemoveByKeyAsync(key, entityOptions, cancellationToken);
    }

    private async Task<T> GetOrSetByKeyAsync<T>(string key, T value, DistributedCacheEntryOptions cacheOptions,
        IEntityOptions<T> entityOptions, CancellationToken cancellationToken)
    {
        var data = await GetByKeyAsync(key, entityOptions, cancellationToken);

        if (data is null)
        {
            await SetByKeyAsync(key, value, cacheOptions, entityOptions, cancellationToken);
        }

        return value;
    }

    private async Task<T> GetOrSetByKeyAsync<T>(string key, Func<Task<T>> valueFactory,
        DistributedCacheEntryOptions cacheOptions, IEntityOptions<T> entityOptions, CancellationToken cancellationToken)
    {
        var data = await GetByKeyAsync(key, entityOptions, cancellationToken);

        if (data is null)
        {
            data = await valueFactory();
            await SetByKeyAsync(key, data, cacheOptions, entityOptions, cancellationToken);
        }

        return data;
    }

    private async Task<T> GetByKeyAsync<T>(string key, IEntityOptions<T> entityOptions,
        CancellationToken cancellationToken)
    {
        if (!entityOptions.IsEnabled)
        {
            return default;
        }

        try
        {
            return await _resiliencePipeline.ExecuteAsync(async token =>
            {
                var data = await _cache.GetStringAsync(key, token);

                return data is null
                    ? default
                    : JsonSerializer.Deserialize<T>(data, JsonSerializationOptions.Default);
            }, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.Log(_loggingSettings.CacheStateLogLevel, e,
                "Getting value from cache has been failed. Message: {Message}", e.Message);
        }

        return default;
    }

    private async Task SetByKeyAsync<T>(string key, T value, DistributedCacheEntryOptions cacheOptions,
        IEntityOptions<T> entityOptions, CancellationToken cancellationToken)
    {
        if (!entityOptions.IsEnabled)
        {
            return;
        }

        try
        {
            await _resiliencePipeline.ExecuteAsync(async token =>
            {
                var data = JsonSerializer.Serialize(value, JsonSerializationOptions.Default);
                await _cache.SetStringAsync(key, data, cacheOptions, token);
            }, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.Log(_loggingSettings.CacheStateLogLevel, e,
                "Setting value to cache has been failed. Message: {Message}",
                e.Message);
        }
    }

    private async Task RemoveByKeyAsync<T>(string key, IEntityOptions<T> entityOptions,
        CancellationToken cancellationToken)
    {
        if (!entityOptions.IsEnabled)
        {
            return;
        }

        try
        {
            await _resiliencePipeline.ExecuteAsync(async token => await _cache.RemoveAsync(key, token),
                cancellationToken);
        }
        catch (Exception e)
        {
            _logger.Log(_loggingSettings.CacheStateLogLevel, e,
                "Removing value from cache has been failed. Message: {Message}", e.Message);
        }
    }

    private IEntityOptions<T> GetEntityOptions<T>()
    {
        return _serviceProvider.GetService<IEntityOptions<T>>() ??
               throw new RegistrationException($"Type {typeof(T).Name} has not been registered");
    }

    private static string GetKey<T>(IEntityOptions<T> entityOptions, string id) => $"{entityOptions.KeyPrefix}-{id}";
}
