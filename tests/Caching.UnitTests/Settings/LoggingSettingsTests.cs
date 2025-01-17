using AutoFixture;
using Caching.Settings;
using FluentAssertions;
using Xunit;

namespace Caching.UnitTests.Settings;

public class LoggingSettingsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void IsValid_Success()
    {
        // arrange
        var loggingSettings = _fixture.Create<LoggingSettings>();

        // act
        var result = loggingSettings.IsValid;

        // assert
        result.Should().BeTrue();
    }
}
