using System;
using AutoFixture;
using Caching.Exceptions;
using FluentAssertions;
using Xunit;

namespace Caching.UnitTests.Exceptions;

public class RegistrationExceptionTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_ErrorMessage_Success()
    {
        // arrange
        var errorMessage = _fixture.Create<string>();

        // act
        var exception = new RegistrationException(errorMessage);

        // assert
        exception.Message.Should().Be(errorMessage);
        exception.InformationTitle.Should().BeNull();
        exception.InformationMessage.Should().BeNull();
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_ErrorMessage_And_InformationMessage_Success()
    {
        // arrange
        var errorMessage = _fixture.Create<string>();
        var informationMessage = _fixture.Create<string>();

        // act
        var exception = new RegistrationException(errorMessage, informationMessage);

        // assert
        exception.Message.Should().Be(errorMessage);
        exception.InformationTitle.Should().BeNull();
        exception.InformationMessage.Should().Be(informationMessage);
    }

    [Fact]
    public void Constructor_ErrorMessage_And_InnerException_Success()
    {
        // arrange
        var errorMessage = _fixture.Create<string>();
        var innerException = _fixture.Create<Exception>();

        // act
        var exception = new RegistrationException(errorMessage, innerException);

        // assert
        exception.Message.Should().Be(errorMessage);
        exception.InformationTitle.Should().BeNull();
        exception.InformationMessage.Should().BeNull();
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Constructor_ErrorMessage_And_InformationMessage_And_InnerException_Success()
    {
        // arrange
        var errorMessage = _fixture.Create<string>();
        var informationMessage = _fixture.Create<string>();
        var innerException = _fixture.Create<Exception>();

        // act
        var exception = new RegistrationException(errorMessage, informationMessage, innerException);

        // assert
        exception.Message.Should().Be(errorMessage);
        exception.InformationTitle.Should().BeNull();
        exception.InformationMessage.Should().Be(informationMessage);
        exception.InnerException.Should().Be(innerException);
    }
}
