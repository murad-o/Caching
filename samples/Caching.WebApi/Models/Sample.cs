using Caching.Abstractions.Key;

namespace Caching.WebApi.Models;

public class Sample : ICacheId
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string GetId() => Id.ToString();
}
