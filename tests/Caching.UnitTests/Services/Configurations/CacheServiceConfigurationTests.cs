using System;
using AutoFixture;
using Caching.Exceptions;
using Caching.Services.Configurations;
using Caching.Settings;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Caching.UnitTests.Services.Configurations;

public class CacheServiceConfigurationTests
{
    private readonly Fixture _fixture = new();

    public CacheServiceConfigurationTests()
    {
        _fixture.Register<IServiceCollection>(() => new ServiceCollection());
    }

    [Fact]
    public void Constructor_Success()
    {
        // arrange && act
        var configuration = _fixture.Create<CacheServiceConfiguration>();

        // assert
        configuration.Should().NotBeNull();
    }

    [Fact]
    public void AddEntity_Success()
    {
        // arrange
        var cacheSettings = _fixture.Create<CacheSettings>();
        var entityName = _fixture.Create<string>();
        cacheSettings.Entities.Add(entityName, _fixture.Create<EntitySettings>());

        var serviceCollections = _fixture.Create<IServiceCollection>();

        var configuration = new CacheServiceConfiguration(cacheSettings, serviceCollections);

        // act
        var returnedConfiguration = configuration.AddEntity<string>(entityName);

        // assert
        returnedConfiguration.Should().NotBeNull()
            .And.BeSameAs(configuration);
    }

    [Fact]
    public void AddEntity_EmptyEntityName_Failure()
    {
        // arrange
        var configuration = _fixture.Create<CacheServiceConfiguration>();
        var entityName = string.Empty;

        // act
        Action action = () => configuration.AddEntity<string>(entityName);

        // assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .WithMessage($"{nameof(entityName)} cannot be null or empty (Parameter 'entityName')");
    }

    [Fact]
    public void AddEntity_EntityConfigNotFound_Failure()
    {
        // arrange
        var configuration = _fixture.Create<CacheServiceConfiguration>();
        var entityName = _fixture.Create<string>();

        // act
        Action action = () => configuration.AddEntity<string>(entityName);

        // assert
        action.Should().ThrowExactly<SettingsValidationExeption>()
            .WithMessage($"EntityName '{entityName}' has not been found in Entities configuration");
    }
}
