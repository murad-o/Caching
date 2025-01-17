using AutoFixture;
using Caching.Settings;
using FluentAssertions;
using Xunit;

namespace Caching.UnitTests.Settings;

public class InMemorySettingsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsValid_InvalidSizeLimit_Success(int sizeLimit)
    {
        // arrange
        var inMemorySettings = _fixture.Build<InMemorySettings>()
            .With(x => x.SizeLimit, sizeLimit).Create();

        // act
        var result = inMemorySettings.IsValid;

        // assert
        result.Should().BeFalse();
    }
}
