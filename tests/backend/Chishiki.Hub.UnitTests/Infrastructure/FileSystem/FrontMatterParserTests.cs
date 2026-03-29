// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Hub.Infrastructure.FileSystem;

namespace Chishiki.Hub.UnitTests.Infrastructure.FileSystem;

[TestFixture, Category("Unit")]
public class FrontMatterParserTests
{
    [Test]
    public void Parse_WithValidFrontMatter_ExtractsMeta()
    {
        const string input =
            "---\nname: my-skill\ndescription: A useful skill\nversion: \"2.0.0\"\n---\n# Content here\n";

        var (meta, _, _) = FrontMatterParser.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(meta["name"],        Is.EqualTo("my-skill"));
            Assert.That(meta["description"], Is.EqualTo("A useful skill"));
            Assert.That(meta["version"],     Is.EqualTo("2.0.0"));
        });
    }

    [Test]
    public void Parse_WithInlineTagList_ExtractsTags()
    {
        const string input = "---\nname: skill\ntags: [csharp, dotnet, aspire]\n---\ncontent\n";

        var (_, tags, _) = FrontMatterParser.Parse(input);

        Assert.That(tags, Is.EquivalentTo(new[] { "csharp", "dotnet", "aspire" }));
    }

    [Test]
    public void Parse_WithMultilineTagList_ExtractsTags()
    {
        const string input = "---\nname: skill\ntags:\n  - csharp\n  - testing\n---\ncontent\n";

        var (_, tags, _) = FrontMatterParser.Parse(input);

        Assert.That(tags, Is.EquivalentTo(new[] { "csharp", "testing" }));
    }

    [Test]
    public void Parse_WithoutFrontMatter_ReturnsEmptyMetaAndOriginalContent()
    {
        const string input = "# Just a plain markdown file\n\nNo front-matter here.";

        var (meta, tags, content) = FrontMatterParser.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(meta,    Is.Empty);
            Assert.That(tags,    Is.Empty);
            Assert.That(content, Is.EqualTo(input));
        });
    }

    [Test]
    public void Parse_ReturnsContentWithoutFrontMatterBlock()
    {
        const string input = "---\nname: test\n---\n# Real content\n";

        var (_, _, content) = FrontMatterParser.Parse(input);

        Assert.That(content, Does.Contain("# Real content"));
        Assert.That(content, Does.Not.Contain("---"));
    }

    [Test]
    public void Parse_QuotedValues_StripsQuotes()
    {
        const string input = "---\nname: \"quoted-name\"\nversion: '1.2.3'\n---\n";

        var (meta, _, _) = FrontMatterParser.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(meta["name"],    Is.EqualTo("quoted-name"));
            Assert.That(meta["version"], Is.EqualTo("1.2.3"));
        });
    }

    [Test]
    public void Parse_EmptyFrontMatterClosingMissing_ReturnsOriginalContent()
    {
        const string input = "---\nname: orphan\n";

        var (meta, _, content) = FrontMatterParser.Parse(input);

        Assert.Multiple(() =>
        {
            Assert.That(meta,    Is.Empty);
            Assert.That(content, Is.EqualTo(input));
        });
    }
}
