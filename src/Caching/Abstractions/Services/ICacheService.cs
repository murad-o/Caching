using System;
using System.Threading;
using System.Threading.Tasks;
using Caching.Abstractions.Key;

namespace Caching.Abstractions.Services;

public interface ICacheService
{
    Task<T> GetAsync<T>(string id, CancellationToken cancellationToken);

    Task<T> GetByKeyAsync<T>(string key, CancellationToken cancellationToken);

    Task SetAsync<T>(T value, CancellationToken cancellationToken) where T : ICacheId;

    Task SetAsync<T>(T value, TimeSpan expirationTimeRelativeToNow, CancellationToken cancellationToken)
        where T : ICacheId;

    Task SetAsync<T>(T value, DateTime expirationDateTime, CancellationToken cancellationToken) where T : ICacheId;

    Task SetSlidingAsync<T>(T value, TimeSpan expirationTimeRelativeToNow, CancellationToken cancellationToken)
        where T : ICacheId;

    Task SetByKeyAsync<T>(string key, T value, CancellationToken cancellationToken);

    Task SetByKeyAsync<T>(string key, T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken);

    Task SetByKeyAsync<T>(string key, T value, DateTime expirationDateTime, CancellationToken cancellationToken);

    Task SetSlidingByKeyAsync<T>(string key, T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken);

    Task<T> GetOrSetAsync<T>(T value, CancellationToken cancellationToken) where T : ICacheId;

    Task<T> GetOrSetAsync<T>(T value, TimeSpan expirationTimeRelativeToNow, CancellationToken cancellationToken)
        where T : ICacheId;

    Task<T> GetOrSetAsync<T>(T value, DateTime expirationDateTime, CancellationToken cancellationToken)
        where T : ICacheId;

    Task<T> GetOrSetSlidingAsync<T>(T value, TimeSpan expirationTimeRelativeToNow, CancellationToken cancellationToken)
        where T : ICacheId;

    Task<T> GetOrSetByKeyAsync<T>(string key, T value, CancellationToken cancellationToken);

    Task<T> GetOrSetByKeyAsync<T>(string key, T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken);

    Task<T> GetOrSetByKeyAsync<T>(string key, T value, DateTime expirationDateTime,
        CancellationToken cancellationToken);

    Task<T> GetOrSetSlidingByKeyAsync<T>(string key, T value, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken);

    Task<T> GetOrSetAsync<T>(string id, Func<Task<T>> valueFactory, CancellationToken cancellationToken);

    Task<T> GetOrSetAsync<T>(string id, Func<Task<T>> valueFactory, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken);

    Task<T> GetOrSetAsync<T>(string id, Func<Task<T>> valueFactory, DateTime expirationDateTime,
        CancellationToken cancellationToken);

    Task<T> GetOrSetSlidingAsync<T>(string id, Func<Task<T>> valueFactory, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken);

    Task<T> GetOrSetByKeyAsync<T>(string key, Func<Task<T>> valueFactory, CancellationToken cancellationToken);

    Task<T> GetOrSetByKeyAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken);

    Task<T> GetOrSetByKeyAsync<T>(string key, Func<Task<T>> valueFactory, DateTime expirationDateTime,
        CancellationToken cancellationToken);

    Task<T> GetOrSetSlidingByKeyAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan expirationTimeRelativeToNow,
        CancellationToken cancellationToken);

    Task RemoveAsync<T>(T value, CancellationToken cancellationToken) where T : ICacheId;

    Task RemoveAsync<T>(string id, CancellationToken cancellationToken);

    Task RemoveByKeyAsync<T>(string key, CancellationToken cancellationToken);
}
