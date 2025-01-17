using Caching.Abstractions.Models;

namespace Caching.Settings;

public class RedisSettings : ISettings
{
    public string Configuration { get; set; }

    public string InstanceName { get; set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Configuration) && !string.IsNullOrWhiteSpace(InstanceName);
}
