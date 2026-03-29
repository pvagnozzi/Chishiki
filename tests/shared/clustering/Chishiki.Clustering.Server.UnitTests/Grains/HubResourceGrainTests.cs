// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Clustering.Server.Grains;
using Chishiki.Hub.Contracts;

namespace Chishiki.Clustering.Server.UnitTests.Grains;

[TestFixture, Category("Unit")]
public class HubResourceGrainTests
{
    private HubResourceGrain _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new HubResourceGrain();

    [Test]
    public async Task GetAsync_InitialState_ReturnsNull()
    {
        var result = await _sut.GetAsync();

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SetAsync_StoresDto_GetAsyncReturnsSameInstance()
    {
        var dto = MakeDto("my-skill", HubResourceType.Skill);

        await _sut.SetAsync(dto);
        var result = await _sut.GetAsync();

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public async Task SetAsync_CalledTwice_ReturnsLatestDto()
    {
        var first  = MakeDto("first",  HubResourceType.Skill);
        var second = MakeDto("second", HubResourceType.Agent);

        await _sut.SetAsync(first);
        await _sut.SetAsync(second);

        var result = await _sut.GetAsync();
        Assert.That(result, Is.SameAs(second));
    }

    [Test]
    public async Task InvalidateAsync_AfterSet_GetAsyncReturnsNull()
    {
        await _sut.SetAsync(MakeDto("cached", HubResourceType.Skill));
        await _sut.InvalidateAsync();

        var result = await _sut.GetAsync();
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task InvalidateAsync_OnEmptyCache_DoesNotThrow()
    {
        Assert.That(async () => await _sut.InvalidateAsync(), Throws.Nothing);
        Assert.That(await _sut.GetAsync(), Is.Null);
    }

    [Test]
    public async Task SetAsync_ThenInvalidate_ThenSet_ReturnsFinalDto()
    {
        var first  = MakeDto("first",  HubResourceType.Skill);
        var second = MakeDto("second", HubResourceType.Skill);

        await _sut.SetAsync(first);
        await _sut.InvalidateAsync();
        await _sut.SetAsync(second);

        Assert.That(await _sut.GetAsync(), Is.SameAs(second));
    }

    private static HubResourceDto MakeDto(string name, HubResourceType type) =>
        new(name, "desc", type, [], "1.0.0", "content", DateTimeOffset.UtcNow);
}
