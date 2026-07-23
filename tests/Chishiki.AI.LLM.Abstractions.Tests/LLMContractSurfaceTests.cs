// -----------------------------------------------------------------------------
// File:        LLMContractSurfaceTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers record and option surface behavior for the LLM abstractions project.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.AI.LLM.Abstractions;
using NUnit.Framework;

namespace Chishiki.AI.LLM.Abstractions.Tests;

/// <summary>Provides coverage for immutable contract records and enum values in the LLM abstraction layer.</summary>
public sealed class LLMContractSurfaceTests
{
    [Test]
    public void ChatOptions_PreserveConfiguredInferenceValues()
    {
        IReadOnlyList<string> stopSequences = ["STOP", "END"];

        var options = new LLMChatOptions
        {
            ModelId = "gpt-demo",
            Temperature = 0.25f,
            TopP = 0.9f,
            MaxTokens = 256,
            StopSequences = stopSequences,
        };

        Assert.Multiple(() =>
        {
            Assert.That(options.ModelId, Is.EqualTo("gpt-demo"));
            Assert.That(options.Temperature, Is.EqualTo(0.25f));
            Assert.That(options.TopP, Is.EqualTo(0.9f));
            Assert.That(options.MaxTokens, Is.EqualTo(256));
            Assert.That(options.StopSequences, Is.SameAs(stopSequences));
        });
    }

    [Test]
    public void EmbeddingOptions_PreserveConfiguredDimensionsAndModel()
    {
        var options = new LLMEmbeddingOptions
        {
            ModelId = "text-embedding-demo",
            Dimensions = 1536,
        };

        Assert.Multiple(() =>
        {
            Assert.That(options.ModelId, Is.EqualTo("text-embedding-demo"));
            Assert.That(options.Dimensions, Is.EqualTo(1536));
        });
    }

    [Test]
    public void ChatResponse_PreservesOptionalMetadataAndUsage()
    {
        var usage = new LLMTokenUsage(12, 34, 46);

        var response = new LLMChatResponse(
            "Hello from the model",
            ModelId: "model-1",
            FinishReason: "stop",
            Usage: usage);

        Assert.Multiple(() =>
        {
            Assert.That(response.Content, Is.EqualTo("Hello from the model"));
            Assert.That(response.ModelId, Is.EqualTo("model-1"));
            Assert.That(response.FinishReason, Is.EqualTo("stop"));
            Assert.That(response.Usage, Is.SameAs(usage));
        });
    }

    [Test]
    public void RagRecords_PreserveDocumentMetadataSourcesAndScores()
    {
        IReadOnlyDictionary<string, string> metadata = new Dictionary<string, string>
        {
            ["source"] = "manual",
            ["locale"] = "en-US",
        };

        var document = new LLMRagDocument("doc-1", "Grounding content", metadata);
        var searchResult = new LLMRagSearchResult(document, 0.87d);
        IReadOnlyList<LLMRagSearchResult> sources = [searchResult];
        var askResult = new LLMRagAskResult("Grounded answer", sources, ModelId: "rag-model");

        Assert.Multiple(() =>
        {
            Assert.That(document.Id, Is.EqualTo("doc-1"));
            Assert.That(document.Text, Is.EqualTo("Grounding content"));
            Assert.That(document.Metadata, Is.SameAs(metadata));
            Assert.That(searchResult.Document, Is.SameAs(document));
            Assert.That(searchResult.RelevanceScore, Is.EqualTo(0.87d));
            Assert.That(askResult.Answer, Is.EqualTo("Grounded answer"));
            Assert.That(askResult.Sources, Is.SameAs(sources));
            Assert.That(askResult.ModelId, Is.EqualTo("rag-model"));
        });
    }

    [Test]
    public void ChatRole_ContainsExpectedStableEnumMembers()
    {
        var values = Enum.GetValues<LLMChatRole>();

        Assert.That(values, Is.EquivalentTo(new[]
        {
            LLMChatRole.System,
            LLMChatRole.User,
            LLMChatRole.Assistant,
            LLMChatRole.Tool,
        }));
    }
}
