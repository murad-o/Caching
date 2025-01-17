using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Caching.Abstractions.Entities;
using Caching.Abstractions.Key;
using Caching.Abstractions.Services;
using Caching.Exceptions;
using Caching.Models;
using Caching.Services;
using Caching.Settings;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Polly;
using Xunit;

namespace Caching.UnitTests.Services;

public class CacheServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IDistributedCache> _distributedCache;
    private readonly ICacheService _cacheService;
    private readonly Mock<IKeyedServiceProvider> _serviceProvider;

    private const string RegistrationExceptionErrorMessage = "Type {0} has not been registered";

    public CacheServiceTests()
    {
        _fixture = new Fixture();
        var mockRepository = new MockRepository(MockBehavior.Strict);
        _distributedCache = mockRepository.Create<IDistributedCache>();

        _fixture.Build<CacheSettings>()
            .With(x => x.IsEnabled, true)
            .Create();

        _fixture.Register<IServiceCollection>(() => new ServiceCollection());

        var logger = mockRepository.Create<ILogger<CacheService>>(MockBehavior.Loose);

        var cacheSettings = _fixture.Create<CacheSettings>();
        var cacheOptions = mockRepository.Create<IOptions<CacheSettings>>();
        cacheOptions.Setup(x => x.Value).Returns(cacheSettings);

        var resiliencePipeline = new ResiliencePipelineBuilder().Build();

        _serviceProvider = mockRepository.Create<IKeyedServiceProvider>();
        _serviceProvider.Setup(x =>
                x.GetRequiredKeyedService(typeof(ResiliencePipeline), ResiliencePipelines.CircuitBreakerPipeline))
            .Returns(resiliencePipeline);

        _cacheService = new CacheService(_distributedCache.Object, _serviceProvider.Object, logger.Object,
            cacheOptions.Object);
    }

    [Fact]
    public void GetAsync_EntitySettingsNotRegistered_Failure()
    {
        // act
        Func<Task> func = () => _cacheService.GetAsync<CacheEntity>(It.IsAny<string>(), CancellationToken.None);

        // assert
        VerifyGetAsync(false, false);
        VerifyRegistrationException(func, nameof(CacheEntity));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task GetAsync_OptionsFromSettings_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        ArrangeGet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.GetAsync<CacheEntity>(It.IsAny<string>(), CancellationToken.None);

        // assert
        VerifyGetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Fact]
    public async Task GetAsync_CacheThrowsException_Success()
    {
        // arrange
        ArrangeGetWithException();

        // act
        var actualResult = await _cacheService.GetAsync<CacheEntity>(It.IsAny<string>(), CancellationToken.None);

        // assert
        VerifyGetWithException(actualResult);
    }

    [Fact]
    public void GetByKeyAsync_EntitySettingsNotRegistered_Failure()
    {
        // act
        Func<Task> func = () => _cacheService.GetByKeyAsync<CacheEntity>(It.IsAny<string>(), CancellationToken.None);

        // assert
        VerifyGetAsync(false, false);
        VerifyRegistrationException(func, nameof(CacheEntity));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task GetByKeyAsync_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        ArrangeGet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.GetByKeyAsync<CacheEntity>(It.IsAny<string>(), CancellationToken.None);

        // assert
        VerifyGetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Fact]
    public async Task GetByKeyAsync_CacheThrowsException_Success()
    {
        // arrange
        ArrangeGetWithException();

        // act
        var actualResult = await _cacheService.GetByKeyAsync<CacheEntity>(It.IsAny<string>(), CancellationToken.None);

        // assert
        VerifyGetWithException(actualResult);
    }

    [Fact]
    public void SetAsync_EntitySettingsNotRegistered_Failure()
    {
        // act
        var func = () => _cacheService.SetAsync(It.IsAny<CacheEntity>(), CancellationToken.None);

        // assert
        VerifySetAsync(false, false);
        VerifyRegistrationException(func, nameof(String));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SetAsync_OptionsFromSettings_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var cacheEntity = _fixture.Create<CacheEntity>();
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.SetAsync(cacheEntity, CancellationToken.None);

        // assert
        VerifySetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SetAsync_TimeSpan_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var cacheEntity = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.SetAsync(cacheEntity, timeSpan, CancellationToken.None);

        // assert
        VerifySetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SetAsync_DateTime_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var cacheEntity = _fixture.Create<CacheEntity>();
        var dateTime = _fixture.Create<DateTime>();
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.SetAsync(cacheEntity, dateTime, CancellationToken.None);

        // assert
        VerifySetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Fact]
    public async Task SetAsync_CacheThrowsException_Success()
    {
        // arrange
        ArrangeSetWithException();
        var cacheEntity = _fixture.Create<CacheEntity>();

        // act
        await _cacheService.SetAsync(cacheEntity, CancellationToken.None);

        // assert
        VerifySetAsync(true, true);
    }

    [Fact]
    public void SetSlidingAsync_EntitySettingsNotRegistered_Failure()
    {
        // arrange
        var timeSpan = _fixture.Create<TimeSpan>();

        // act
        var func = () => _cacheService.SetSlidingAsync(It.IsAny<CacheEntity>(), timeSpan, CancellationToken.None);

        // assert
        VerifySetAsync(false, false);
        VerifyRegistrationException(func, nameof(String));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SetSlidingAsync_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var cacheEntity = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.SetSlidingAsync(cacheEntity, timeSpan, CancellationToken.None);

        // assert
        VerifySetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Fact]
    public async Task SetSlidingAsync_CacheThrowsException_Success()
    {
        // arrange
        ArrangeSetWithException();
        var cacheEntity = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();

        // act
        await _cacheService.SetSlidingAsync(cacheEntity, timeSpan, CancellationToken.None);

        // assert
        VerifySetAsync(true, true);
    }

    [Fact]
    public void SetByKeyAsync_EntitySettingsNotRegistered_Failure()
    {
        // act
        var func = () => _cacheService.SetByKeyAsync(It.IsAny<string>(), It.IsAny<CacheEntity>(), CancellationToken.None);

        // assert
        VerifySetAsync(false, false);
        VerifyRegistrationException(func, nameof(String));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SetByKeyAsync_OptionsFromSettings_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var key = _fixture.Create<string>();
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.SetByKeyAsync(key, It.IsAny<CacheEntity>(), CancellationToken.None);

        // assert
        VerifySetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SetByKeyAsync_TimeSpan_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var key = _fixture.Create<string>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.SetByKeyAsync(key, It.IsAny<CacheEntity>(), timeSpan, CancellationToken.None);

        // assert
        VerifySetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SetByKeyAsync_DateTime_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var key = _fixture.Create<string>();
        var dateTime = _fixture.Create<DateTime>();
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.SetByKeyAsync(key, It.IsAny<CacheEntity>(), dateTime, CancellationToken.None);

        // assert
        VerifySetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Fact]
    public async Task SetByKeyAsync_CacheThrowsException_Success()
    {
        // arrange
        ArrangeSetWithException();
        var cacheEntity = _fixture.Create<CacheEntity>();
        var key = _fixture.Create<string>();

        // act
        await _cacheService.SetByKeyAsync(key, cacheEntity, CancellationToken.None);

        // assert
        VerifySetAsync(true, true);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SetSlidingByKeyAsync_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var key = _fixture.Create<string>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.SetSlidingByKeyAsync(key, It.IsAny<CacheEntity>(), timeSpan, CancellationToken.None);

        // assert
        VerifySetAsync(isCacheEnabled, isEntityCacheEnabled);
    }

    [Fact]
    public void GetOrSetAsync_EntitySettingsNotRegistered_Failure()
    {
        // act
        Func<Task> func = () => _cacheService.GetOrSetAsync(It.IsAny<CacheEntity>(), CancellationToken.None);

        // assert
        VerifySetAsync(false, false);
        VerifyRegistrationException(func, nameof(String));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetAsync_OptionsFromSettings_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetAsync(value, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetAsync_TimeSpan_Success(bool isCacheEnabled, bool isEntityCacheEnabled, bool isFoundData)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetAsync(value, timeSpan, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetAsync_DateTime_Success(bool isCacheEnabled, bool isEntityCacheEnabled, bool isFoundData)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        var dateTime = _fixture.Create<DateTime>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetAsync(value, dateTime, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetSlidingAsync_Success(bool isCacheEnabled, bool isEntityCacheEnabled, bool isFoundData)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetSlidingAsync(value, timeSpan, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Fact]
    public void GetOrSetByKeyAsync_EntitySettingsNotRegistered_Failure()
    {
        // act
        Func<Task> func = () =>
            _cacheService.GetOrSetByKeyAsync(It.IsAny<string>(), It.IsAny<CacheEntity>(), CancellationToken.None);

        // assert
        VerifySetAsync(false, false);
        VerifyRegistrationException(func, nameof(String));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetByKeyAsync_OptionsFromSettings_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<CacheEntity>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetByKeyAsync(key, value, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetByKeyAsync_TimeSpan_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetByKeyAsync(key, value, timeSpan, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetByKeyAsync_DateTime_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<CacheEntity>();
        var dateTime = _fixture.Create<DateTime>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetByKeyAsync(key, value, dateTime, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetByKeySlidingAsync_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetSlidingByKeyAsync(key, value, timeSpan, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Fact]
    public void GetOrSetAsync_Delegate_EntitySettingsNotRegistered_Failure()
    {
        // act
        Func<Task> func = () => _cacheService.GetOrSetAsync(It.IsAny<string>(), GetCacheEntity, CancellationToken.None);

        // assert
        VerifySetAsync(false, false);
        VerifyRegistrationException(func, nameof(String));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetAsync_Delegate_OptionsFromSettings_Success(bool isCacheEnabled,
        bool isEntityCacheEnabled, bool isFoundData)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetAsync(It.IsAny<string>(), GetCacheEntity, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetAsync_Delegate_TimeSpan_Success(bool isCacheEnabled,
        bool isEntityCacheEnabled, bool isFoundData)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetAsync(It.IsAny<string>(), GetCacheEntity, timeSpan, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetAsync_Delegate_DateTime_Success(bool isCacheEnabled,
        bool isEntityCacheEnabled, bool isFoundData)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        var dateTime = _fixture.Create<DateTime>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetAsync(It.IsAny<string>(), GetCacheEntity, dateTime, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetSlidingAsync_Delegate_TimeSpan_Success(bool isCacheEnabled,
        bool isEntityCacheEnabled, bool isFoundData)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        var timeSpan = _fixture.Create<TimeSpan>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetSlidingAsync(It.IsAny<string>(), GetCacheEntity, timeSpan, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Fact]
    public void GetOrSetByKeyAsync_Delegate_EntitySettingsNotRegistered_Failure()
    {
        // act
        Func<Task> func = () => _cacheService.GetOrSetByKeyAsync(It.IsAny<string>(), GetCacheEntity, CancellationToken.None);

        // assert
        VerifySetAsync(false, false);
        VerifyRegistrationException(func, nameof(String));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetByKeyAsync_Delegate_OptionsFromSettings_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var key = _fixture.Create<string>();
        var value = _fixture.Create<CacheEntity>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetByKeyAsync(key, GetCacheEntity, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetByKeyAsync_Delegate_TimeSpan_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var key = _fixture.Create<string>();
        var timeSpan = _fixture.Create<TimeSpan>();
        var value = _fixture.Create<CacheEntity>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetByKeyAsync(key, GetCacheEntity, timeSpan, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetByKeyAsync_Delegate_DateTime_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var key = _fixture.Create<string>();
        var dateTime = _fixture.Create<DateTime>();
        var value = _fixture.Create<CacheEntity>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetByKeyAsync(key, GetCacheEntity, dateTime, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task GetOrSetByKeySlidingAsync_Delegate_Success(bool isCacheEnabled, bool isEntityCacheEnabled,
        bool isFoundData)
    {
        // arrange
        var key = _fixture.Create<string>();
        var timeSpan = _fixture.Create<TimeSpan>();
        var value = _fixture.Create<CacheEntity>();
        ArrangeGetOrSet(value, isCacheEnabled, isEntityCacheEnabled, isFoundData);

        // act
        await _cacheService.GetOrSetSlidingByKeyAsync(key, GetCacheEntity, timeSpan, CancellationToken.None);

        // assert
        VerifyGetOrSetAsync(isCacheEnabled, isEntityCacheEnabled, isFoundData);
    }

    [Fact]
    public void RemoveAsync_EntitySettingsNotRegistered_Failure()
    {
        // act
        var func = () => _cacheService.RemoveAsync(It.IsAny<CacheEntity>(), CancellationToken.None);

        // assert
        VerifyRemove(false, false);
        VerifyRegistrationException(func, nameof(CacheEntity));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task RemoveAsync_OptionsFromSettings_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var value = _fixture.Create<CacheEntity>();
        ArrangeRemove(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.RemoveAsync(value, CancellationToken.None);

        // assert
        VerifyRemove(isCacheEnabled, isEntityCacheEnabled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task RemoveAsync_Id_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var id = _fixture.Create<string>();
        ArrangeRemove(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.RemoveAsync<CacheEntity>(id, CancellationToken.None);

        // assert
        VerifyRemove(isCacheEnabled, isEntityCacheEnabled);
    }

    [Fact]
    public async Task RemoveAsync_CacheThrowsException_Success()
    {
        // arrange
        ArrangeRemoveWithException();
        var value = _fixture.Create<CacheEntity>();

        // act
        await _cacheService.RemoveAsync(value, CancellationToken.None);

        // assert
        VerifyRemove(true, true);
    }

    [Fact]
    public void RemoveByKeyAsync_EntitySettingsNotRegistered_Failure()
    {
        // act
        var func = () => _cacheService.RemoveByKeyAsync<CacheEntity>(It.IsAny<string>(), CancellationToken.None);

        // assert
        VerifyRemove(false, false);
        VerifyRegistrationException(func, nameof(CacheEntity));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task RemoveByKeyAsync_Success(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        // arrange
        var key = _fixture.Create<string>();
        ArrangeRemove(isCacheEnabled, isEntityCacheEnabled);

        // act
        await _cacheService.RemoveByKeyAsync<CacheEntity>(key, CancellationToken.None);

        // assert
        VerifyRemove(isCacheEnabled, isEntityCacheEnabled);
    }

    [Fact]
    public async Task RemoveByKeyAsync_CacheThrowsException_Success()
    {
        // arrange
        ArrangeRemoveWithException();
        var key = _fixture.Create<string>();

        // act
        await _cacheService.RemoveByKeyAsync<CacheEntity>(key, CancellationToken.None);

        // assert
        VerifyRemove(true, true);
    }

    private void ArrangeGet(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        ArrangeCacheSettings(isCacheEnabled, isEntityCacheEnabled);

        _distributedCache.Setup(x =>
            x.GetAsync(It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(It.IsAny<byte[]>());
    }

    private void ArrangeGetWithException()
    {
        ArrangeCacheSettings(true, true);

        _distributedCache.Setup(x =>
            x.GetAsync(It.IsAny<string>(), CancellationToken.None)).ThrowsAsync(new Exception());
    }

    private void ArrangeSet(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        ArrangeCacheSettings(isCacheEnabled, isEntityCacheEnabled);

        _distributedCache.Setup(x =>
            x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                CancellationToken.None)).Returns(Task.CompletedTask);
    }

    private void ArrangeSetWithException()
    {
        ArrangeCacheSettings(true, true);

        _distributedCache.Setup(x =>
            x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                CancellationToken.None)).ThrowsAsync(new Exception());
    }

    private void ArrangeGetOrSet(CacheEntity value, bool isCacheEnabled, bool isEntityCacheEnabled, bool isFoundData)
    {
        ArrangeSet(isCacheEnabled, isEntityCacheEnabled);

        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        _distributedCache.Setup(x => x.GetAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(isFoundData ? bytes : null);
    }

    private void ArrangeRemove(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        ArrangeCacheSettings(isCacheEnabled, isEntityCacheEnabled);

        _distributedCache.Setup(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None))
            .Returns(Task.CompletedTask);
    }

    private void ArrangeRemoveWithException()
    {
        ArrangeCacheSettings(true, true);

        _distributedCache.Setup(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None))
            .ThrowsAsync(new Exception());
    }

    private void ArrangeCacheSettings(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        var entityOptions = _fixture.Build<EntityOptions<CacheEntity>>()
            .With(x => x.IsEnabled, isCacheEnabled && isEntityCacheEnabled)
            .Create();

        _serviceProvider.Setup(x => x.GetService(typeof(IEntityOptions<CacheEntity>))).Returns(entityOptions);
    }

    private void VerifyGetAsync(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        _distributedCache.Verify(x => x.GetAsync(It.IsAny<string>(), CancellationToken.None),
            isEntityCacheEnabled && isCacheEnabled ? Times.Once : Times.Never);
    }

    private void VerifyGetWithException(CacheEntity actualResult)
    {
        VerifyGetAsync(true, true);
        actualResult.Should().Be(default);
    }

    private void VerifySetAsync(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        _distributedCache.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                CancellationToken.None), isEntityCacheEnabled && isCacheEnabled ? Times.Once : Times.Never);
    }

    private void VerifyGetOrSetAsync(bool isCacheEnabled, bool isEntityCacheEnabled, bool isFoundData)
    {
        VerifyGetAsync(isCacheEnabled, isEntityCacheEnabled);
        _distributedCache.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                CancellationToken.None), isEntityCacheEnabled && isCacheEnabled && !isFoundData ? Times.Once : Times.Never);
    }

    private static void VerifyRegistrationException(Func<Task> func, string type)
    {
        func.Should().ThrowExactlyAsync<RegistrationException>()
            .WithMessage(string.Format(RegistrationExceptionErrorMessage, type));
    }

    private void VerifyRemove(bool isCacheEnabled, bool isEntityCacheEnabled)
    {
        _distributedCache.Verify(x => x.RemoveAsync(It.IsAny<string>(), CancellationToken.None),
            isCacheEnabled && isEntityCacheEnabled ? Times.Once : Times.Never);
    }

    private Task<CacheEntity> GetCacheEntity() => Task.FromResult(_fixture.Create<CacheEntity>());

    private class CacheEntity : ICacheId
    {
        public string Id { get; set; }

        public string GetId() => Id;
    }
}
