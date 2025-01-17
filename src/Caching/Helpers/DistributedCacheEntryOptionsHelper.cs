using System;
using Caching.Abstractions.Entities;
using Caching.Enums;
using Microsoft.Extensions.Caching.Distributed;

namespace Caching.Helpers;

public static class DistributedCacheEntryOptionsHelper
{
    public static DistributedCacheEntryOptions GetCacheOptions<T>(IEntityOptions<T> entityOptions)
    {
        var lifeTime = TimeSpan.FromSeconds(entityOptions.LifeTimeInSeconds);
        var options = new DistributedCacheEntryOptions();

        if (entityOptions.LifeTimeType == LifeTimeTypeEnum.ExpirationTimeRelativeToNow)
        {
            options.SetAbsoluteExpiration(lifeTime);
        }
        else
        {
            options.SetSlidingExpiration(lifeTime);
        }

        return options;
    }
}
