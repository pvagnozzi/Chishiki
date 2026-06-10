// -----------------------------------------------------------------------------
// File:        SKEmbeddingClientTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers SK-backed embedding success, failure, and edge-case behaviors.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.AI.LLM.SemanticKernel.Embedding;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.AI.LLM.SemanticKernel.Tests;

/// <summary>Provides focused tests for <see cref="SKEmbeddingClient"/>.</summary>
public sealed class SKEmbeddingClientTests
{
    [Test]
    public async Task GenerateEmbeddingAsync_ReturnsTheFirstGeneratedVector()
    {
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator
            .GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CreateGeneratedEmbeddings(new float[] { 1f, 2f, 3f }.AsMemory(), new float[] { 9f, 9f, 9f }.AsMemory())));

        var client = new SKEmbeddingClient(generator, NullLogger<SKEmbeddingClient>.Instance);

        var embedding = await client.GenerateEmbeddingAsync("hello");

        Assert.That(embedding.ToArray(), Is.EqualTo(new[] { 1f, 2f, 3f }));
    }

    [Test]
    public async Task GenerateEmbeddingsAsync_ReturnsVectorsInInputOrder()
    {
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator
            .GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(CreateGeneratedEmbeddings(new float[] { 1f, 2f }.AsMemory(), new float[] { 3f, 4f }.AsMemory())));

        var client = new SKEmbeddingClient(generator, NullLogger<SKEmbeddingClient>.Instance);

        var embeddings = await client.GenerateEmbeddingsAsync(["first", "second"]);

        Assert.Multiple(() =>
        {
            Assert.That(embeddings, Has.Count.EqualTo(2));
            Assert.That(embeddings[0].ToArray(), Is.EqualTo(new[] { 1f, 2f }));
            Assert.That(embeddings[1].ToArray(), Is.EqualTo(new[] { 3f, 4f }));
        });
    }

    [Test]
    public void GenerateEmbeddingsAsync_WhenGeneratorThrows_PropagatesTheException()
    {
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        generator
            .GenerateAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<EmbeddingGenerationOptions?>(), Arg.Any<CancellationToken>())
            .Returns<Task<GeneratedEmbeddings<Embedding<float>>>>(_ => throw new InvalidOperationException("embedding failed"));

        var client = new SKEmbeddingClient(generator, NullLogger<SKEmbeddingClient>.Instance);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            _ = await client.GenerateEmbeddingsAsync(["first"]));

        Assert.That(exception?.Message, Is.EqualTo("embedding failed"));
    }

    [Test]
    public void GenerateEmbeddingAsync_WhenCanceled_ThrowsOperationCanceledException()
    {
        var generator = Substitute.For<IEmbeddingGenerator<string, Embedding<float>>>();
        var client = new SKEmbeddingClient(generator, NullLogger<SKEmbeddingClient>.Instance);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            _ = await client.GenerateEmbeddingAsync("hello", cancellationToken: cts.Token));

        Assert.That(exception, Is.Not.Null);
    }

    private static GeneratedEmbeddings<Embedding<float>> CreateGeneratedEmbeddings(params ReadOnlyMemory<float>[] vectors)
        => new(vectors.Select(vector => new Embedding<float>(vector)).ToArray());
}
