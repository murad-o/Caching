using Caching.Abstractions.Models;

namespace Caching.Settings;

public class InMemorySettings : ISettings
{
    public long SizeLimit { get; set; }

    public bool IsValid => SizeLimit > 0;
}
