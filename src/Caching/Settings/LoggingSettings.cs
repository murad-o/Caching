using System;
using Caching.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Caching.Settings;

public class LoggingSettings : ISettings
{
    public LogLevel CacheStateLogLevel { get; set; } = LogLevel.Information;

    public bool IsValid => Enum.IsDefined(CacheStateLogLevel);
}
