using Caching.Abstractions.Services;
using Caching.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Caching.WebApi.Controllers;

[ApiController]
[ProducesResponseType(StatusCodes.Status200OK)]
[Route("samples")]
public class SampleController(ICacheService cacheService) : ControllerBase
{
    private readonly ICacheService _cacheService = cacheService;

    [HttpGet("no-key")]
    public async Task<Sample> GetAsync(string id)
    {
        var value = await _cacheService.GetAsync<Sample>(id, CancellationToken.None);
        return value;
    }

    [HttpGet]
    public async Task<Sample> GetByKeyAsync(string key)
    {
        var sample = await _cacheService.GetByKeyAsync<Sample>(key, CancellationToken.None);
        return sample;
    }

    [HttpPost]
    public async Task<IActionResult> SetByKeyAsync(string key, Sample sample)
    {
        await _cacheService.SetByKeyAsync(key, sample, CancellationToken.None);
        return Ok();
    }

    [HttpPost("no-key")]
    public async Task<IActionResult> SetAsync(Sample sample)
    {
        await _cacheService.SetAsync(sample, CancellationToken.None);
        return Ok();
    }

    [HttpPost("sliding")]
    public async Task<IActionResult> SetSlidingByKeyAsync(string key, Sample sample)
    {
        await _cacheService.SetSlidingByKeyAsync(key, sample, TimeSpan.FromMinutes(1), CancellationToken.None);
        return Ok();
    }

    [HttpGet("get-or-set")]
    public async Task<Sample[]> GetOrSetByKeyAsync(string key)
    {
        var samples =
            await _cacheService.GetOrSetByKeyAsync(key, GetSamplesAsync, CancellationToken.None);
        return samples;
    }

    [HttpPost("get-or-set")]
    public async Task<Sample> GetOrSetByKeyAsync(string key, Sample sample)
    {
        var data = await _cacheService.GetOrSetByKeyAsync(key, sample, TimeSpan.FromMinutes(2),
            CancellationToken.None);
        return data;
    }

    [HttpGet("get-or-set-no-key")]
    public async Task<Sample> GetOrSetAsync(Sample sample)
    {
        var data = await _cacheService.GetOrSetAsync(sample, CancellationToken.None);
        return data;
    }

    [HttpGet("get-or-set-sliding")]
    public async Task<Sample[]> GetOrSetSlidingByKeyAsync(string key)
    {
        var samples = await _cacheService.GetOrSetSlidingByKeyAsync(key, GetSamplesAsync, TimeSpan.FromMinutes(2),
            CancellationToken.None);
        return samples;
    }

    [HttpPost("get-or-set-sliding")]
    public async Task<Sample> GetOrSetSlidingByKeyAsync(string key, Sample sample)
    {
        var data = await _cacheService.GetOrSetSlidingByKeyAsync(key, sample, TimeSpan.FromMinutes(2),
            CancellationToken.None);
        return data;
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveByKeyAsync(string key)
    {
        await _cacheService.RemoveByKeyAsync<Sample>(key, CancellationToken.None);
        return Ok();
    }

    [HttpDelete("no-key")]
    public async Task<IActionResult> RemoveAsync(Sample sample)
    {
        await _cacheService.RemoveAsync(sample, CancellationToken.None);
        return Ok();
    }

    private static Task<Sample[]> GetSamplesAsync()
    {
        return Task.FromResult(new[]
        {
            new Sample { Id = 1, Name = "Sample1" },
            new Sample { Id = 2, Name = "Sample2" },
            new Sample { Id = 3, Name = "Sample3" }
        });
    }
}
