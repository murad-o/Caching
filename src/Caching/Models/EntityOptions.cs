using Caching.Abstractions.Entities;
using Caching.Enums;

namespace Caching.Models;

public class EntityOptions<T> : IEntityOptions<T>
{
    public int LifeTimeInSeconds { get; set; }

    public LifeTimeTypeEnum LifeTimeType { get; set; }

    public string KeyPrefix { get; set; }

    public bool IsEnabled { get; set; }
}
