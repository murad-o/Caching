using System;
using System.IO;
using System.Text;
using AutoFixture;
using Caching.Abstractions.Services;
using Caching.Enums;
using Caching.Exceptions;
using Caching.Extensions;
using Caching.Settings;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Caching.UnitTests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private readonly IFixture _fixture;
    private readonly IConfiguration _configuration;

    public ServiceCollectionExtensionsTests()
    {
        _fixture = new Fixture();
        _fixture.Register<IServiceCollection>(() => new ServiceCollection());

        _configuration = new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes("{}")))
            .Build();
    }

    [Theory]
    [InlineData($"{nameof(CacheSourceTypeEnum.Redis)}")]
    [InlineData($"{nameof(CacheSourceTypeEnum.InMemory)}")]
    public void AddCache_Success(string cacheSourceType)
    {
        // arrange
        var serviceCollections = _fixture.Create<IServiceCollection>();

        _configuration[$"{CacheSettings.SectionName}:{nameof(CacheSettings.CacheSourceType)}"] = cacheSourceType;
        _configuration[$"{CacheSettings.SectionName}:Redis:{nameof(RedisSettings.Configuration)}"] =
            _fixture.Create<Uri>().ToString();
        _configuration[$"{CacheSettings.SectionName}:Redis:{nameof(RedisSettings.InstanceName)}"] =
            _fixture.Create<string>();
        _configuration[$"{CacheSettings.SectionName}:InMemory:{nameof(InMemorySettings.SizeLimit)}"] = 1000.ToString();

        // act
        var result = serviceCollections.AddCache(_configuration);

        // assert
        result.Should().NotBeNull();

        serviceCollections.Should()
            .ContainSingle(sd =>
                sd.ServiceType == typeof(IDistributedCache)
                && sd.Lifetime == ServiceLifetime.Singleton);

        serviceCollections.Should()
            .ContainSingle(sd =>
                sd.ServiceType == typeof(ICacheService)
                && sd.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddCache_InvalidEntities_Failure()
    {
        // arrange
        var serviceCollections = _fixture.Create<IServiceCollection>();

        _configuration[$"{CacheSettings.SectionName}:{nameof(CacheSettings.CacheSourceType)}"] =
            nameof(CacheSourceTypeEnum.Redis);
        _configuration[$"{CacheSettings.SectionName}:Redis:{nameof(RedisSettings.Configuration)}"] =
            _fixture.Create<Uri>().ToString();
        _configuration[$"{CacheSettings.SectionName}:Redis:{nameof(RedisSettings.InstanceName)}"] =
            _fixture.Create<string>();
        _configuration[$"{CacheSettings.SectionName}:Entities:sample:LifeTimeInSeconds"] = "300";
        _configuration[$"{CacheSettings.SectionName}:Entities:sample:LifeTimeType"] = "ExpirationTimeRelativeToNow";
        _configuration[$"{CacheSettings.SectionName}:Entities:sample:KeyPrefix"] = string.Empty;

        // act
        Action action = () => serviceCollections.AddCache(_configuration);

        // assert
        action.Should()
            .ThrowExactly<SettingsValidationExeption>()
            .WithMessage(ServiceCollectionExtensions.CacheConfigurationErrorMessage);
    }
}
