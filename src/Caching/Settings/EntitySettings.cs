using Caching.Abstractions.Models;
using Caching.Enums;

namespace Caching.Settings;

public class EntitySettings : ISettings
{
    public int LifeTimeInSeconds { get; set; }

    public LifeTimeTypeEnum LifeTimeType { get; set; }

    public string KeyPrefix { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsValid => LifeTimeInSeconds > 0 && !string.IsNullOrWhiteSpace(KeyPrefix);
}
