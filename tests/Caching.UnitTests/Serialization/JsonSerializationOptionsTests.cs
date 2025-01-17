using System.Text.Json;
using Caching.Serialization;
using FluentAssertions;
using Xunit;

namespace Caching.UnitTests.Serialization;

public class JsonSerializationOptionsTests
{
    [Fact]
    public void Default_Success()
    {
        // act
        var options = JsonSerializationOptions.Default;

        // assert
        options.Should().BeOfType<JsonSerializerOptions>()
            .Which.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
    }
}
