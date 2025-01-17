namespace Caching.Abstractions.Configurations;

public interface ICacheServiceConfiguration
{
    ICacheServiceConfiguration AddEntity<T>(string entityName) where T : class;
}
