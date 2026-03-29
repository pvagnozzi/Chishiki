// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Hub.Contracts;
using Chishiki.Hub.Contracts.Requests;
using Chishiki.Hub.Infrastructure.FileSystem;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chishiki.Hub.UnitTests.Infrastructure.FileSystem;

[TestFixture, Category("Unit")]
public class FileSystemHubResourceServiceTests
{
    private string _repoRoot = null!;
    private FileSystemHubResourceService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repoRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_repoRoot);
        _sut = new FileSystemHubResourceService(
            _repoRoot,
            NullLogger<FileSystemHubResourceService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_repoRoot))
            Directory.Delete(_repoRoot, recursive: true);
    }

    // ── ListResourcesAsync ──────────────────────────────────────────────────

    [Test]
    public async Task ListResourcesAsync_DirectoryDoesNotExist_ReturnsEmptyList()
    {
        var result = await _sut.ListResourcesAsync(HubResourceType.Skill);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task ListResourcesAsync_WithSkills_ReturnsAllSkills()
    {
        CreateSkillFile("csharp-async", "Async programming patterns", ["dotnet", "async"]);
        CreateSkillFile("ef-core", "Entity Framework Core best practices", ["dotnet", "ef"]);

        var result = await _sut.ListResourcesAsync(HubResourceType.Skill);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(r => r.Name), Is.EquivalentTo(new[] { "csharp-async", "ef-core" }));
    }

    [Test]
    public async Task ListResourcesAsync_WithTagFilter_ReturnsOnlyMatchingResources()
    {
        CreateSkillFile("csharp-async", "Async skill", ["dotnet", "async"]);
        CreateSkillFile("ef-core", "EF Core skill", ["dotnet", "ef"]);

        var result = await _sut.ListResourcesAsync(HubResourceType.Skill, tag: "async");

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("csharp-async"));
    }

    [Test]
    public async Task ListResourcesAsync_TagFilterCaseInsensitive_MatchesRegardlessOfCase()
    {
        CreateSkillFile("csharp-async", "Async skill", ["DOTNET", "ASYNC"]);

        var result = await _sut.ListResourcesAsync(HubResourceType.Skill, tag: "async");

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ListResourcesAsync_WithAgents_ReturnsAgentDtos()
    {
        CreateAgentFile("dotnet-api", "ASP.NET Core API agent", ["aspnet", "csharp"]);

        var result = await _sut.ListResourcesAsync(HubResourceType.Agent);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Type, Is.EqualTo(HubResourceType.Agent));
        Assert.That(result[0].Name, Is.EqualTo("dotnet-api"));
    }

    [Test]
    public async Task ListResourcesAsync_WithInstructions_PopulatesMetadata()
    {
        CreateInstructionFile("csharp", "C# coding instructions", ["csharp"], "2.0.0");

        var result = await _sut.ListResourcesAsync(HubResourceType.Instruction);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Description, Is.EqualTo("C# coding instructions"));
            Assert.That(result[0].Version, Is.EqualTo("2.0.0"));
            Assert.That(result[0].Tags, Contains.Item("csharp"));
        });
    }

    // ── GetResourceAsync ────────────────────────────────────────────────────

    [Test]
    public async Task GetResourceAsync_ExistingSkill_ReturnsDtoWithCorrectFields()
    {
        CreateSkillFile("csharp-async", "Async programming patterns", ["dotnet", "async"], "1.2.0");

        var result = await _sut.GetResourceAsync(HubResourceType.Skill, "csharp-async");

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Name, Is.EqualTo("csharp-async"));
            Assert.That(result.Description, Is.EqualTo("Async programming patterns"));
            Assert.That(result.Type, Is.EqualTo(HubResourceType.Skill));
            Assert.That(result.Version, Is.EqualTo("1.2.0"));
            Assert.That(result.Tags, Is.EquivalentTo(new[] { "dotnet", "async" }));
        });
    }

    [Test]
    public async Task GetResourceAsync_NameLookupIsCaseInsensitive()
    {
        CreateSkillFile("csharp-async", "Async skill", []);

        var result = await _sut.GetResourceAsync(HubResourceType.Skill, "CSHARP-ASYNC");

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetResourceAsync_NonExistentResource_ReturnsNull()
    {
        var result = await _sut.GetResourceAsync(HubResourceType.Skill, "no-such-skill");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetResourceAsync_ContentDoesNotContainFrontMatterBlock()
    {
        CreateSkillFile("my-skill", "A skill", ["tag"]);

        var result = await _sut.GetResourceAsync(HubResourceType.Skill, "my-skill");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Content, Does.Not.Contain("---"));
    }

    // ── CreateResourceAsync ─────────────────────────────────────────────────

    [Test]
    public async Task CreateResourceAsync_CreatesFileOnDisk()
    {
        var request = new CreateResourceRequest(
            HubResourceType.Instruction,
            "new-instruction",
            "A new instruction file",
            ["csharp", "testing"]);

        await _sut.CreateResourceAsync(request);

        var expectedPath = Path.Combine(_repoRoot, ".github", "instructions", "new-instruction.instructions.md");
        Assert.That(File.Exists(expectedPath), Is.True);
    }

    [Test]
    public async Task CreateResourceAsync_ReturnsCreatedDto()
    {
        var request = new CreateResourceRequest(
            HubResourceType.Skill,
            "new-skill",
            "A brand new skill",
            ["dotnet"]);

        var result = await _sut.CreateResourceAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("new-skill"));
            Assert.That(result.Description, Is.EqualTo("A brand new skill"));
            Assert.That(result.Type, Is.EqualTo(HubResourceType.Skill));
            Assert.That(result.Version, Is.EqualTo("1.0.0"));
            Assert.That(result.Tags, Contains.Item("dotnet"));
        });
    }

    [Test]
    public async Task CreateResourceAsync_CreatedSkillIsSubsequentlyListable()
    {
        var request = new CreateResourceRequest(
            HubResourceType.Skill, "roundtrip-skill", "Roundtrip test", ["test"]);

        await _sut.CreateResourceAsync(request);
        var listed = await _sut.ListResourcesAsync(HubResourceType.Skill);

        Assert.That(listed.Any(r => r.Name == "roundtrip-skill"), Is.True);
    }

    // ── SearchResourcesAsync ─────────────────────────────────────────────────

    [Test]
    public async Task SearchResourcesAsync_QueryMatchesName_ReturnsResult()
    {
        CreateSkillFile("csharp-async", "Async programming", []);

        var result = await _sut.SearchResourcesAsync(
            new SearchResourcesRequest("async"));

        Assert.That(result.Any(r => r.Name == "csharp-async"), Is.True);
    }

    [Test]
    public async Task SearchResourcesAsync_QueryMatchesDescription_ReturnsResult()
    {
        CreateSkillFile("ef-core", "Entity Framework Core patterns", ["ef"]);

        var result = await _sut.SearchResourcesAsync(
            new SearchResourcesRequest("Entity Framework"));

        Assert.That(result.Any(r => r.Name == "ef-core"), Is.True);
    }

    [Test]
    public async Task SearchResourcesAsync_QueryMatchesTag_ReturnsResult()
    {
        CreateSkillFile("csharp-async", "Async skill", ["async", "dotnet"]);

        var result = await _sut.SearchResourcesAsync(
            new SearchResourcesRequest("dotnet"));

        Assert.That(result.Any(r => r.Name == "csharp-async"), Is.True);
    }

    [Test]
    public async Task SearchResourcesAsync_NoMatch_ReturnsEmpty()
    {
        CreateSkillFile("csharp-async", "Async skill", ["dotnet"]);

        var result = await _sut.SearchResourcesAsync(
            new SearchResourcesRequest("nosuchterm_xyz"));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SearchResourcesAsync_TypeFilter_LimitsScope()
    {
        CreateSkillFile("csharp-async", "Async skill", []);
        CreateAgentFile("csharp-agent", "Agent for csharp", []);

        var result = await _sut.SearchResourcesAsync(
            new SearchResourcesRequest("csharp", Type: HubResourceType.Skill));

        Assert.That(result.All(r => r.Type == HubResourceType.Skill), Is.True);
    }

    [Test]
    public async Task SearchResourcesAsync_TagsFilter_RequiresAllTagsPresent()
    {
        CreateSkillFile("skill-ab", "Skill with both tags", ["alpha", "beta"]);
        CreateSkillFile("skill-a", "Skill with only alpha", ["alpha"]);

        var result = await _sut.SearchResourcesAsync(
            new SearchResourcesRequest("skill", Tags: ["alpha", "beta"]));

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("skill-ab"));
    }

    // ── ApplyCollectionAsync ─────────────────────────────────────────────────

    [Test]
    public async Task ApplyCollectionAsync_CollectionNotFound_ThrowsInvalidOperationException()
    {
        Assert.That(
            async () => await _sut.ApplyCollectionAsync("missing-collection"),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task ApplyCollectionAsync_ValidCollection_ReturnsReferencedResources()
    {
        CreateSkillFile("csharp-async", "Async skill", []);
        CreateCollectionFile("my-set", "My collection", ["Skill/csharp-async"]);

        var result = await _sut.ApplyCollectionAsync("my-set");

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("csharp-async"));
    }

    // ── ScaffoldProjectAsync ──────────────────────────────────────────────────

    [Test]
    public void ScaffoldProjectAsync_ThrowsNotImplementedException()
    {
        Assert.That(
            async () => await _sut.ScaffoldProjectAsync(
                new ScaffoldProjectRequest("dotnet-api", "/tmp/out")),
            Throws.TypeOf<NotImplementedException>());
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private void CreateSkillFile(
        string name, string description, string[] tags, string version = "1.0.0")
    {
        var dir = Path.Combine(_repoRoot, ".github", "skills", name);
        Directory.CreateDirectory(dir);
        var tagList = string.Join(", ", tags.Select(t => $"\"{t}\""));
        File.WriteAllText(
            Path.Combine(dir, "SKILL.md"),
            $"---\nname: {name}\ndescription: {description}\ntags: [{tagList}]\nversion: \"{version}\"\n---\n# {name}\n\n{description}\n");
    }

    private void CreateAgentFile(string name, string description, string[] tags)
    {
        var dir = Path.Combine(_repoRoot, ".github", "agents");
        Directory.CreateDirectory(dir);
        var tagList = string.Join(", ", tags.Select(t => $"\"{t}\""));
        File.WriteAllText(
            Path.Combine(dir, $"{name}.chatmode.md"),
            $"---\nname: {name}\ndescription: {description}\ntags: [{tagList}]\nversion: \"1.0.0\"\n---\n# {name}\n");
    }

    private void CreateInstructionFile(
        string name, string description, string[] tags, string version = "1.0.0")
    {
        var dir = Path.Combine(_repoRoot, ".github", "instructions");
        Directory.CreateDirectory(dir);
        var tagList = string.Join(", ", tags.Select(t => $"\"{t}\""));
        File.WriteAllText(
            Path.Combine(dir, $"{name}.instructions.md"),
            $"---\nname: {name}\ndescription: {description}\ntags: [{tagList}]\nversion: \"{version}\"\n---\n# {name}\n");
    }

    private void CreateCollectionFile(string name, string description, string[] items)
    {
        var dir = Path.Combine(_repoRoot, "hub", "collections");
        Directory.CreateDirectory(dir);
        var itemLines = string.Join("\n", items.Select(i => $"- {i}"));
        File.WriteAllText(
            Path.Combine(dir, $"{name}.yaml"),
            $"---\nname: {name}\ndescription: {description}\ntags: []\nversion: \"1.0.0\"\n---\n{itemLines}\n");
    }
}
