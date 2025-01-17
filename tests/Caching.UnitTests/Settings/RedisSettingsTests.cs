using AutoFixture;
using Caching.Settings;
using FluentAssertions;
using Xunit;

namespace Caching.UnitTests.Settings;

public class RedisSettingsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Theory]
    [InlineData("http://a", "prefix", true)]
    [InlineData("http://a", "", false)]
    [InlineData("", "prefix", false)]
    [InlineData("", "", false)]
    public void IsValid_Success(string configuration, string instanceName, bool isValid)
    {
        // arrange
        var redisSettings = _fixture.Build<RedisSettings>()
            .With(x => x.Configuration, configuration)
            .With(x => x.InstanceName, instanceName)
            .Create();

        // act
        var result = redisSettings.IsValid;

        // assert
        result.Should().Be(isValid);
    }

    [Fact]
    public void IsValid_InvalidConfiguration_Success()
    {
        // arrange
        var redisSettings = _fixture.Build<RedisSettings>()
            .With(x => x.Configuration, string.Empty)
            .Create();

        // act
        var result = redisSettings.IsValid;

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_InvalidInstanceName_Success()
    {
        // arrange
        var redisSettings = _fixture.Build<RedisSettings>()
            .With(x => x.InstanceName, string.Empty).Create();

        // act
        var result = redisSettings.IsValid;

        // assert
        result.Should().BeFalse();
    }
}
