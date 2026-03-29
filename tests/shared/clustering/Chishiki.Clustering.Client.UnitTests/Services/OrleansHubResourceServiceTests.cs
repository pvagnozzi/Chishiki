// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Clustering.Client.Services;
using Chishiki.Clustering.Grains;
using Chishiki.Hub.Contracts;
using Chishiki.Hub.Contracts.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Chishiki.Clustering.Client.UnitTests.Services;

[TestFixture, Category("Unit")]
public class OrleansHubResourceServiceTests
{
    private IGrainFactory _grainFactory = null!;
    private IHubResourceGrain _grain = null!;
    private IHubResourceService _inner = null!;
    private OrleansHubResourceService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _grain       = Substitute.For<IHubResourceGrain>();
        _grainFactory = Substitute.For<IGrainFactory>();
        _grainFactory
            .GetGrain<IHubResourceGrain>(Arg.Any<string>(), Arg.Any<string?>())
            .Returns(_grain);

        _inner = Substitute.For<IHubResourceService>();
        _sut   = new OrleansHubResourceService(
            _grainFactory,
            _inner,
            NullLogger<OrleansHubResourceService>.Instance);
    }

    // ── GetResourceAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetResourceAsync_CacheHit_ReturnsCachedDtoWithoutCallingInner()
    {
        var cached = MakeDto("csharp-async", HubResourceType.Skill);
        _grain.GetAsync().Returns(cached);

        var result = await _sut.GetResourceAsync(HubResourceType.Skill, "csharp-async");

        Assert.That(result, Is.SameAs(cached));
        await _inner.DidNotReceive()
            .GetResourceAsync(Arg.Any<HubResourceType>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetResourceAsync_CacheMiss_CallsInnerAndCachesResult()
    {
        var dto = MakeDto("csharp-async", HubResourceType.Skill);
        _grain.GetAsync().Returns((HubResourceDto?)null);
        _inner.GetResourceAsync(HubResourceType.Skill, "csharp-async", Arg.Any<CancellationToken>())
              .Returns(dto);

        var result = await _sut.GetResourceAsync(HubResourceType.Skill, "csharp-async");

        Assert.That(result, Is.SameAs(dto));
        await _grain.Received(1).SetAsync(dto);
    }

    [Test]
    public async Task GetResourceAsync_CacheMissAndInnerReturnsNull_DoesNotSetCache()
    {
        _grain.GetAsync().Returns((HubResourceDto?)null);
        _inner.GetResourceAsync(Arg.Any<HubResourceType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((HubResourceDto?)null);

        var result = await _sut.GetResourceAsync(HubResourceType.Skill, "missing");

        Assert.That(result, Is.Null);
        await _grain.DidNotReceive().SetAsync(Arg.Any<HubResourceDto>());
    }

    [Test]
    public async Task GetResourceAsync_UsesCompositeGrainKey()
    {
        _grain.GetAsync().Returns((HubResourceDto?)null);
        _inner.GetResourceAsync(Arg.Any<HubResourceType>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
              .Returns((HubResourceDto?)null);

        await _sut.GetResourceAsync(HubResourceType.Agent, "dotnet-api");

        _grainFactory.Received(1)
            .GetGrain<IHubResourceGrain>("Agent:dotnet-api", Arg.Any<string?>());
    }

    // ── CreateResourceAsync ───────────────────────────────────────────────────

    [Test]
    public async Task CreateResourceAsync_CreatesViaInnerAndSetsCache()
    {
        var dto = MakeDto("new-skill", HubResourceType.Skill);
        var request = new CreateResourceRequest(HubResourceType.Skill, "new-skill", "desc", []);
        _inner.CreateResourceAsync(request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.CreateResourceAsync(request);

        Assert.That(result, Is.SameAs(dto));
        await _grain.Received(1).SetAsync(dto);
    }

    // ── List / Search / Apply / Scaffold always delegate to inner ─────────────

    [Test]
    public async Task ListResourcesAsync_AlwaysDelegatesToInner()
    {
        _inner.ListResourcesAsync(HubResourceType.Skill, null, Arg.Any<CancellationToken>())
              .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        await _sut.ListResourcesAsync(HubResourceType.Skill);

        await _inner.Received(1)
            .ListResourcesAsync(HubResourceType.Skill, null, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SearchResourcesAsync_AlwaysDelegatesToInner()
    {
        var req = new SearchResourcesRequest("query");
        _inner.SearchResourcesAsync(req, Arg.Any<CancellationToken>())
              .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        await _sut.SearchResourcesAsync(req);

        await _inner.Received(1).SearchResourcesAsync(req, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ApplyCollectionAsync_DelegatesToInner()
    {
        _inner.ApplyCollectionAsync("my-collection", Arg.Any<CancellationToken>())
              .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        await _sut.ApplyCollectionAsync("my-collection");

        await _inner.Received(1).ApplyCollectionAsync("my-collection", Arg.Any<CancellationToken>());
    }

    private static HubResourceDto MakeDto(string name, HubResourceType type) =>
        new(name, "desc", type, [], "1.0.0", "content", DateTimeOffset.UtcNow);
}
