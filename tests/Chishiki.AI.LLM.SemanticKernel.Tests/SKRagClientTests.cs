// -----------------------------------------------------------------------------
// File:        SKRagClientTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers the current SK RAG placeholder behavior and provider default option values.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.AI.LLM.Abstractions;
using Chishiki.AI.LLM.SemanticKernel.Providers;
using Chishiki.AI.LLM.SemanticKernel.Rag;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.AI.LLM.SemanticKernel.Tests;

/// <summary>Provides focused tests for the current SK RAG surface and provider defaults.</summary>
public sealed class SKRagClientTests
{
    [Test]
    public void ProviderOptions_ExposeExpectedRecommendedDefaults()
    {
        var openAi = new OpenAIOptions();
        var ollama = new OllamaOptions();
        var azure = new AzureOpenAIOptions();

        Assert.Multiple(() =>
        {
            Assert.That(openAi.ChatModelId, Is.EqualTo("gpt-4o-mini"));
            Assert.That(openAi.EmbeddingModelId, Is.EqualTo("text-embedding-3-small"));
            Assert.That(ollama.ChatModelId, Is.EqualTo("llama3"));
            Assert.That(ollama.EmbeddingModelId, Is.EqualTo("nomic-embed-text"));
            Assert.That(ollama.Endpoint, Is.EqualTo("http://localhost:11434"));
            Assert.That(azure.ChatModelId, Is.EqualTo("gpt-4o"));
            Assert.That(azure.EmbeddingModelId, Is.EqualTo("text-embedding-3-small"));
            Assert.That(azure.AuthMethod, Is.EqualTo(AzureOpenAIAuthMethod.ApiKey));
        });
    }

    [Test]
    public async Task RagOperations_CurrentlyThrowTheDocumentedNotImplementedException()
    {
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        var client = new SKRagClient(generator, NullLogger<SKRagClient>.Instance);
        var document = new LLMRagDocument("doc-1", "content", new Dictionary<string, string> { ["source"] = "test" });

        async Task AssertPendingAsync(Func<Task> action)
        {
            var exception = Assert.ThrowsAsync<NotImplementedException>(async () => await action());
            Assert.That(exception?.Message, Does.Contain("VectorStore integration pending"));
        }

        await AssertPendingAsync(() => client.IngestAsync("docs", document));
        await AssertPendingAsync(() => client.IngestBatchAsync("docs", [document]));
        await AssertPendingAsync(() => client.DeleteAsync("docs", "doc-1"));
        await AssertPendingAsync(async () => _ = await client.SearchAsync("docs", "question"));
        await AssertPendingAsync(async () => _ = await client.AskAsync("docs", "question"));
    }
}
