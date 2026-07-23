// -----------------------------------------------------------------------------
// File:        MapsterMapperTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers focused mapper behavior tests for the Chishiki.Mapping project.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using InnerMapper = MapsterMapper.IMapper;

namespace Chishiki.Mapping.Tests;

/// <summary>Provides focused tests for <see cref="Chishiki.Mapping.MapsterMapper"/>.</summary>
[TestFixture]
public sealed class MapsterMapperTests
{
    [Test]
    public void MapToNewDestinationUsesInnerMapperAndReturnsMappedInstance()
    {
        var innerMapper = Substitute.For<InnerMapper>();
        var source = new SourceModel { Name = "Ada" };
        var expected = new DestinationModel { Name = "Ada" };
        innerMapper.Map<SourceModel, DestinationModel>(source).Returns(expected);
        var sut = new Chishiki.Mapping.MapsterMapper(innerMapper, NullLogger<Chishiki.Mapping.MapsterMapper>.Instance);

        var result = sut.MapTo<DestinationModel, SourceModel>(source);

        Assert.That(result, Is.SameAs(expected));
        _ = innerMapper.Received(1).Map<SourceModel, DestinationModel>(source);
    }

    [Test]
    public void MapToExistingDestinationUsesInnerMapperAndReturnsUpdatedInstance()
    {
        var innerMapper = Substitute.For<InnerMapper>();
        var source = new SourceModel { Name = "Grace" };
        var destination = new DestinationModel { Name = "Old" };
        innerMapper.Map(source, destination).Returns(destination);
        var sut = new Chishiki.Mapping.MapsterMapper(innerMapper, NullLogger<Chishiki.Mapping.MapsterMapper>.Instance);

        var result = sut.MapTo<DestinationModel, SourceModel>(source, destination);

        Assert.That(result, Is.SameAs(destination));
        _ = innerMapper.Received(1).Map(source, destination);
    }

    [Test]
    public void MapToWhenInnerMapperThrowsRethrowsException()
    {
        var innerMapper = Substitute.For<InnerMapper>();
        var source = new SourceModel { Name = "Ada" };
        innerMapper.Map<SourceModel, DestinationModel>(source).Returns(_ => throw new InvalidOperationException("boom"));
        var sut = new Chishiki.Mapping.MapsterMapper(innerMapper, NullLogger<Chishiki.Mapping.MapsterMapper>.Instance);

        Assert.That(
            () => sut.MapTo<DestinationModel, SourceModel>(source),
            Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("boom"));
    }

    private sealed class SourceModel
    {
        public string Name { get; init; } = string.Empty;
    }

    private sealed class DestinationModel
    {
        public string Name { get; init; } = string.Empty;
    }
}
