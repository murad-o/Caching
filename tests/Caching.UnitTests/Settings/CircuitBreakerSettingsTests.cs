using AutoFixture;
using Caching.Settings;
using FluentAssertions;
using Xunit;

namespace Caching.UnitTests.Settings;

public class CircuitBreakerSettingsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void IsValid_Success()
    {
        var circuitBreakerSettings = GetCircuitBreakerSettings();

        // act
        var result = circuitBreakerSettings.IsValid;

        // assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(-120)]
    [InlineData(0)]
    public void IsValid_InvalidSamplingDurationInSeconds_Success(int samplingDurationInSeconds)
    {
        var circuitBreakerSettings = GetCircuitBreakerSettings();
        circuitBreakerSettings.SamplingDurationInSeconds = samplingDurationInSeconds;

        // act
        var result = circuitBreakerSettings.IsValid;

        // assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(-120)]
    [InlineData(0)]
    public void IsValid_InvalidBreakDurationInSeconds_Success(int breakDurationInSeconds)
    {
        var circuitBreakerSettings = GetCircuitBreakerSettings();
        circuitBreakerSettings.BreakDurationInSeconds = breakDurationInSeconds;

        // act
        var result = circuitBreakerSettings.IsValid;

        // assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public void IsValid_InvalidMinimumThroughput_Success(int minimumThroughput)
    {
        var circuitBreakerSettings = GetCircuitBreakerSettings();
        circuitBreakerSettings.MinimumThroughput = minimumThroughput;

        // act
        var result = circuitBreakerSettings.IsValid;

        // assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1.5)]
    public void IsValid_InvalidFailureRatio_Success(int failureRatio)
    {
        var circuitBreakerSettings = GetCircuitBreakerSettings();
        circuitBreakerSettings.FailureRatio = failureRatio;

        // act
        var result = circuitBreakerSettings.IsValid;

        // assert
        result.Should().BeFalse();
    }

    private CircuitBreakerSettings GetCircuitBreakerSettings()
    {
        return _fixture.Build<CircuitBreakerSettings>()
            .With(x => x.FailureRatio, 0.5)
            .Create();
    }
}
