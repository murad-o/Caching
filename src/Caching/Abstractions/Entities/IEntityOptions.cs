using Caching.Enums;

namespace Caching.Abstractions.Entities;

public interface IEntityOptions<T>
{
    int LifeTimeInSeconds { get; set; }

    LifeTimeTypeEnum LifeTimeType { get; set; }

    string KeyPrefix { get; set; }

    bool IsEnabled { get; set; }
}
