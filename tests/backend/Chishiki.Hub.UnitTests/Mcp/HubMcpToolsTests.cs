// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Hub.Contracts;
using Chishiki.Hub.Contracts.Requests;
using Chishiki.Hub.Mcp;
using NSubstitute;

namespace Chishiki.Hub.UnitTests.Mcp;

[TestFixture, Category("Unit")]
public class HubMcpToolsTests
{
    private IHubResourceService _service = null!;
    private HubMcpTools _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _service = Substitute.For<IHubResourceService>();
        _sut = new HubMcpTools(_service);
    }

    [Test]
    public async Task ListResourcesAsync_ValidType_DelegatesToService()
    {
        _service.ListResourcesAsync(HubResourceType.Skill, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        var result = await _sut.ListResourcesAsync("skill");

        await _service.Received(1)
            .ListResourcesAsync(HubResourceType.Skill, null, Arg.Any<CancellationToken>());
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ListResourcesAsync_TypeIsCaseInsensitive_ParsesCorrectly()
    {
        _service.ListResourcesAsync(Arg.Any<HubResourceType>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        await _sut.ListResourcesAsync("AGENT");

        await _service.Received(1)
            .ListResourcesAsync(HubResourceType.Agent, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ListResourcesAsync_WithTag_ForwardsTagToService()
    {
        _service.ListResourcesAsync(HubResourceType.Agent, "csharp", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        await _sut.ListResourcesAsync("Agent", tag: "csharp");

        await _service.Received(1)
            .ListResourcesAsync(HubResourceType.Agent, "csharp", Arg.Any<CancellationToken>());
    }

    [Test]
    public void ListResourcesAsync_InvalidType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.ListResourcesAsync("not-a-type"));
    }

    [Test]
    public async Task GetResourceAsync_ValidInput_ReturnsServiceResult()
    {
        var dto = MakeDto("my-skill", HubResourceType.Skill);
        _service.GetResourceAsync(HubResourceType.Skill, "my-skill", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<HubResourceDto?>(dto));

        var result = await _sut.GetResourceAsync("skill", "my-skill");

        Assert.That(result, Is.EqualTo(dto));
    }

    [Test]
    public async Task CreateResourceAsync_ParsesCommaSeparatedTags()
    {
        var dto = MakeDto("agent-x", HubResourceType.Agent);
        _service.CreateResourceAsync(Arg.Any<CreateResourceRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(dto));

        await _sut.CreateResourceAsync("agent", "agent-x", "desc", "csharp, testing");

        await _service.Received(1).CreateResourceAsync(
            Arg.Is<CreateResourceRequest>(r =>
                r.Type == HubResourceType.Agent &&
                r.Name == "agent-x" &&
                r.Tags.Contains("csharp") &&
                r.Tags.Contains("testing")),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task CreateResourceAsync_NullTags_PassesEmptyTagList()
    {
        var dto = MakeDto("t", HubResourceType.Template);
        _service.CreateResourceAsync(Arg.Any<CreateResourceRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(dto));

        await _sut.CreateResourceAsync("template", "t", "desc");

        await _service.Received(1).CreateResourceAsync(
            Arg.Is<CreateResourceRequest>(r => r.Tags.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SearchResourcesAsync_WithoutType_PassesNullType()
    {
        _service.SearchResourcesAsync(Arg.Any<SearchResourcesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        await _sut.SearchResourcesAsync("aspire");

        await _service.Received(1).SearchResourcesAsync(
            Arg.Is<SearchResourcesRequest>(r => r.Query == "aspire" && r.Type == null),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SearchResourcesAsync_WithType_ParsesTypeAndForwards()
    {
        _service.SearchResourcesAsync(Arg.Any<SearchResourcesRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        await _sut.SearchResourcesAsync("dotnet", "skill");

        await _service.Received(1).SearchResourcesAsync(
            Arg.Is<SearchResourcesRequest>(r => r.Type == HubResourceType.Skill),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ApplyCollectionAsync_DelegatesToService()
    {
        _service.ApplyCollectionAsync("base", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<HubResourceDto>>([]));

        await _sut.ApplyCollectionAsync("base");

        await _service.Received(1).ApplyCollectionAsync("base", Arg.Any<CancellationToken>());
    }

    private static HubResourceDto MakeDto(string name, HubResourceType type) =>
        new(name, "desc", type, [], "1.0.0", "content", DateTimeOffset.UtcNow);
}
