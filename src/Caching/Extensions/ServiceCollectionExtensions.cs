using System;
using System.Threading.Tasks;
using Caching.Abstractions.Configurations;
using Caching.Abstractions.Services;
using Caching.Enums;
using Caching.Exceptions;
using Caching.Models;
using Caching.Services;
using Caching.Services.Configurations;
using Caching.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;

namespace Caching.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CacheConfigurationErrorMessage = $"{CacheSettings.SectionName} configuration is not valid";

    public static ICacheServiceConfiguration AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheSettings = GetCacheSettings(configuration);

        switch (cacheSettings.CacheSourceType)
        {
            case CacheSourceTypeEnum.Redis:
                services.AddStackExchangeRedisCache(options =>
                {
                    options.ConfigurationOptions = ConfigurationOptions.Parse(cacheSettings.Redis.Configuration);
                    options.InstanceName = $"{cacheSettings.Redis.InstanceName}.";
                });
                break;
            case CacheSourceTypeEnum.InMemory:
                services.AddDistributedMemoryCache(options =>
                {
                    options.SizeLimit = cacheSettings.InMemory.SizeLimit;
                });
                break;
        }

        services.AddSingleton(_ => cacheSettings);
        services.AddScoped<ICacheService, CacheService>();

        var cacheServiceConfiguration = new CacheServiceConfiguration(cacheSettings, services);
        services.AddSingleton<ICacheServiceConfiguration, CacheServiceConfiguration>(_ => cacheServiceConfiguration);

        AddCircuitBreakerPipeline(services, cacheSettings);

        return cacheServiceConfiguration;
    }

    private static CacheSettings GetCacheSettings(IConfiguration configuration)
    {
        var cacheSettings = configuration.GetSection(CacheSettings.SectionName)
            .Get<CacheSettings>();

        if (cacheSettings is null || !cacheSettings.IsValid)
        {
            throw new SettingsValidationExeption(CacheConfigurationErrorMessage);
        }

        return cacheSettings;
    }

    private static void AddCircuitBreakerPipeline(this IServiceCollection services, CacheSettings cacheSettings)
    {
        services.AddResiliencePipeline(ResiliencePipelines.CircuitBreakerPipeline, (builder, context) =>
        {
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = cacheSettings.CircuitBreaker.FailureRatio,
                SamplingDuration = TimeSpan.FromSeconds(cacheSettings.CircuitBreaker.SamplingDurationInSeconds),
                BreakDuration = TimeSpan.FromSeconds(cacheSettings.CircuitBreaker.BreakDurationInSeconds),
                MinimumThroughput = cacheSettings.CircuitBreaker.MinimumThroughput,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                OnOpened = x =>
                {
                    var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipeline>>();
                    logger.Log(cacheSettings.Logging.CacheStateLogLevel,
                        "Circuit breaker opened. Duration: {Duration}", x.BreakDuration);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipeline>>();
                    logger.Log(cacheSettings.Logging.CacheStateLogLevel, "Circuit breaker closed");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = _ =>
                {
                    var logger = context.ServiceProvider.GetService<ILogger<ResiliencePipeline>>();
                    logger.Log(cacheSettings.Logging.CacheStateLogLevel, "Circuit breaker half opened");
                    return ValueTask.CompletedTask;
                }
            });
        });
    }
}
