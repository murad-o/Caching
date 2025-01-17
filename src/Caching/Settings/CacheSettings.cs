using System.Collections.Generic;
using System.Linq;
using Caching.Abstractions.Models;
using Caching.Enums;

namespace Caching.Settings;

public class CacheSettings : ISettings
{
    public const string SectionName = "Cache";

    public CacheSourceTypeEnum CacheSourceType { get; set; }

    public bool IsEnabled { get; set; } = true;

    public Dictionary<string, EntitySettings> Entities { get; set; } = new();

    public RedisSettings Redis { get; set; }

    public InMemorySettings InMemory { get; set; }

    public LoggingSettings Logging { get; set; } = new();

    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();

    public bool IsValid => Entities.All(x => x.Value.IsValid)
                           && (CacheSourceType == CacheSourceTypeEnum.Redis
                               ? Redis?.IsValid ?? false
                               : InMemory?.IsValid ?? false)
                           && Logging.IsValid
                           && CircuitBreaker.IsValid;
}
