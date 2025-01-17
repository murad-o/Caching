using AutoFixture;
using Caching.Settings;
using FluentAssertions;
using Xunit;

namespace Caching.UnitTests.Settings;

public class EntitySettingsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Theory]
    [InlineData(120, "abc", true)]
    [InlineData(0, "abc", false)]
    [InlineData(360, "", false)]
    public void IsValid_Success(int lifeTimeInSeconds, string keyPrefix, bool isValid)
    {
        // arrange
        var entitySettings = _fixture.Build<EntitySettings>()
            .With(x => x.LifeTimeInSeconds, lifeTimeInSeconds)
            .With(x => x.KeyPrefix, keyPrefix)
            .Create();

        // act
        var result = entitySettings.IsValid;

        // assert
        result.Should().Be(isValid);
    }

    [Theory]
    [InlineData(-120)]
    [InlineData(0)]
    public void IsValid_InvalidLifeTimeInSeconds_Success(int lifeTimeInSeconds)
    {
        // arrange
        var entitySettings = _fixture.Build<EntitySettings>()
            .With(x => x.LifeTimeInSeconds, lifeTimeInSeconds)
            .Create();

        // act
        var result = entitySettings.IsValid;

        // assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void IsValid_InvalidKeyPrefix_Success(string keyPrefix)
    {
        // arrange
        var entitySettings = _fixture.Build<EntitySettings>()
            .With(x => x.KeyPrefix, keyPrefix)
            .Create();

        // act
        var result = entitySettings.IsValid;

        // assert
        result.Should().BeFalse();
    }
}
