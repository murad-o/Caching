using System;
using Caching.Abstractions.Configurations;
using Caching.Abstractions.Entities;
using Caching.Exceptions;
using Caching.Models;
using Caching.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Caching.Services.Configurations;

public class CacheServiceConfiguration(CacheSettings cacheSettings, IServiceCollection services)
    : ICacheServiceConfiguration
{
    private readonly CacheSettings _cacheSettings = cacheSettings;
    private readonly IServiceCollection _services = services;

    public ICacheServiceConfiguration AddEntity<T>(string entityName) where T : class
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new ArgumentNullException(nameof(entityName), $"{nameof(entityName)} cannot be null or empty");
        }

        if (!_cacheSettings.Entities.TryGetValue(entityName, out var entitySettings))
        {
            throw new SettingsValidationExeption(
                $"EntityName '{entityName}' has not been found in {nameof(cacheSettings.Entities)} configuration");
        }

        var entityOptions = new EntityOptions<T>
        {
            KeyPrefix = entitySettings.KeyPrefix,
            LifeTimeInSeconds = entitySettings.LifeTimeInSeconds,
            LifeTimeType = entitySettings.LifeTimeType,
            IsEnabled = _cacheSettings.IsEnabled && entitySettings.IsEnabled
        };

        _services.AddSingleton<IEntityOptions<T>>(entityOptions);

        return this;
    }
}
