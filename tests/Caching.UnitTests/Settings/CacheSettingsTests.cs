using System;
using System.Collections.Generic;
using AutoFixture;
using Caching.Settings;
using FluentAssertions;
using Xunit;

namespace Caching.UnitTests.Settings;

public class CacheSettingsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void IsValid_Success()
    {
        // arrange
        var redisSettings = _fixture.Build<RedisSettings>()
            .With(x => x.Configuration, _fixture.Create<Uri>().ToString)
            .Create();

        var entitySettings = _fixture.Create<EntitySettings>();
        var entities = new Dictionary<string, EntitySettings> { { _fixture.Create<string>(), entitySettings } };

        var circuitBreakerSettings = _fixture.Build<CircuitBreakerSettings>()
            .With(x => x.FailureRatio, 0.5)
            .Create();

        var cacheSettings = _fixture.Build<CacheSettings>()
            .With(x => x.Redis, redisSettings)
            .With(x => x.Entities, entities)
            .With(x => x.CircuitBreaker, circuitBreakerSettings)
            .Create();

        // act
        var result = cacheSettings.IsValid;

        // assert
        result.Should().BeTrue();
    }
}
