using Caching.Abstractions.Models;

namespace Caching.Settings;

public class CircuitBreakerSettings : ISettings
{
    private const int MinimumThroughputMinValue = 2;
    private const double FailureRatioMaxValue = 1.0;

    private const int DefaultSamplingDurationInSeconds = 30;
    private const int DefaultBreakDurationInSeconds = 30;
    private const int DefaultMinimumThroughput = 3;
    private const double DefaultFailureRatio = 0.1;

    /// <summary>
    /// Duration of the sampling over which failure ratios are assessed
    /// </summary>
    public int SamplingDurationInSeconds { get; set; } = DefaultSamplingDurationInSeconds;

    /// <summary>
    /// Duration of break the circuit will stay open before resetting
    /// </summary>
    public int BreakDurationInSeconds { get; set; } = DefaultBreakDurationInSeconds;

    /// <summary>
    /// Minimum number of assessed errors before circuit breaker opens
    /// </summary>
    public int MinimumThroughput { get; set; } = DefaultMinimumThroughput;

    /// <summary>
    /// Failure-to-success ratio at which the circuit will break
    /// </summary>
    public double FailureRatio { get; set; } = DefaultFailureRatio;

    public bool IsValid => SamplingDurationInSeconds > 0 && BreakDurationInSeconds > 0 &&
                           MinimumThroughput >= MinimumThroughputMinValue &&
                           FailureRatio is >= 0 and <= FailureRatioMaxValue;
}
