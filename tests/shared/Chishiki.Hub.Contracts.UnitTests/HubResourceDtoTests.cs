// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Hub.Contracts;
using Chishiki.Hub.Contracts.Requests;

namespace Chishiki.Hub.Contracts.UnitTests;

[TestFixture, Category("Unit")]
public class HubResourceDtoTests
{
    [Test]
    public void HubResourceDto_RecordEquality_WorksCorrectly()
    {
        var now = DateTimeOffset.UtcNow;
        var tags = new[] { "csharp", "testing" };

        var a = new HubResourceDto("my-skill", "A skill", HubResourceType.Skill, tags, "1.0.0", "content", now);
        var b = new HubResourceDto("my-skill", "A skill", HubResourceType.Skill, tags, "1.0.0", "content", now);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void HubResourceType_ParseFromString_IsCaseInsensitive()
    {
        var values = new[] { "template", "Agent", "HOOK", "Instruction", "skill", "Collection" };

        foreach (var value in values)
        {
            Assert.That(
                Enum.TryParse<HubResourceType>(value, ignoreCase: true, out _),
                Is.True,
                $"'{value}' should parse successfully");
        }
    }

    [Test]
    public void CreateResourceRequest_TagsPreserved()
    {
        var tags = new[] { "dotnet", "aspire" };
        var request = new CreateResourceRequest(HubResourceType.Agent, "my-agent", "Desc", tags);

        Assert.That(request.Tags, Is.EquivalentTo(tags));
    }

    [Test]
    public void SearchResourcesRequest_WithNullType_HasNullType()
    {
        var request = new SearchResourcesRequest("aspire");

        Assert.That(request.Type, Is.Null);
        Assert.That(request.Tags, Is.Null);
    }
}
