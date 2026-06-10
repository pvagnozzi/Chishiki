// -----------------------------------------------------------------------------
// File:        SKChatClientTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers SK-backed chat completion success, failure, and streaming edge cases.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.AI.LLM.Abstractions;
using Chishiki.AI.LLM.SemanticKernel.Chat;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.AI.LLM.SemanticKernel.Tests;

/// <summary>Provides focused tests for <see cref="SKChatClient"/>.</summary>
public sealed class SKChatClientTests
{
    [Test]
    public async Task CompleteAsync_MapsContentModelFinishReasonAndUsage()
    {
        var completion = CreateChatMessageContent(
            content: "Hello from SK",
            modelId: "demo-model",
            metadata: new Dictionary<string, object?>
            {
                ["FinishReason"] = "stop",
                ["PromptTokenCount"] = 4,
                ["CompletionTokenCount"] = 6,
            });
        var chatService = new FakeChatCompletionService
        {
            GetContentsAsync = (_, _, _, _) => Task.FromResult<IReadOnlyList<ChatMessageContent>>([completion])
        };

        var client = new SKChatClient(chatService, NullLogger<SKChatClient>.Instance);

        var response = await client.CompleteAsync(
            [LLMChatMessage.System("rules"), LLMChatMessage.User("hello")],
            new LLMChatOptions
            {
                ModelId = "demo-model",
                Temperature = 0.2f,
                TopP = 0.8f,
                MaxTokens = 128,
                StopSequences = ["END"]
            });

        Assert.Multiple(() =>
        {
            Assert.That(response.Content, Is.EqualTo("Hello from SK"));
            Assert.That(response.ModelId, Is.EqualTo("demo-model"));
            Assert.That(response.FinishReason, Is.EqualTo("stop"));
            Assert.That(response.Usage, Is.EqualTo(new LLMTokenUsage(4, 6, 10)));
        });
    }

    [Test]
    public async Task CompleteAsync_WithNullContentAndNoUsage_ReturnsEmptyContentAndNullUsage()
    {
        var completion = CreateChatMessageContent(content: null, modelId: "demo-model");
        var chatService = new FakeChatCompletionService
        {
            GetContentsAsync = (_, _, _, _) => Task.FromResult<IReadOnlyList<ChatMessageContent>>([completion])
        };

        var client = new SKChatClient(chatService, NullLogger<SKChatClient>.Instance);

        var response = await client.CompleteAsync([LLMChatMessage.User("hello")]);

        Assert.Multiple(() =>
        {
            Assert.That(response.Content, Is.EqualTo(string.Empty));
            Assert.That(response.ModelId, Is.EqualTo("demo-model"));
            Assert.That(response.Usage, Is.Null);
        });
    }

    [Test]
    public void CompleteAsync_WhenCanceled_ThrowsOperationCanceledException()
    {
        var chatService = Substitute.For<IChatCompletionService>();
        var client = new SKChatClient(chatService, NullLogger<SKChatClient>.Instance);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            _ = await client.CompleteAsync([LLMChatMessage.User("hello")], cancellationToken: cts.Token));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public async Task CompleteStreamingAsync_StreamsAllChunksAndAppendsFinalChunk()
    {
        var chatService = Substitute.For<IChatCompletionService>();
        chatService
            .GetStreamingChatMessageContentsAsync(Arg.Any<ChatHistory>(), Arg.Any<PromptExecutionSettings?>(), Arg.Any<Kernel?>(), Arg.Any<CancellationToken>())
            .Returns(CreateStreamingSequence(
                CreateStreamingChatMessageContent("Hel", "stream-model"),
                CreateStreamingChatMessageContent("lo", "stream-model")));

        var client = new SKChatClient(chatService, NullLogger<SKChatClient>.Instance);

        var chunks = await client.CompleteStreamingAsync([LLMChatMessage.User("hello")]).ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(chunks.Select(c => c.Content), Is.EqualTo(new[] { "Hel", "lo", string.Empty }));
            Assert.That(chunks[^1].IsFinal, Is.True);
            Assert.That(chunks[0].ModelId, Is.EqualTo("stream-model"));
        });
    }

    [Test]
    public void CompleteStreamingAsync_WhenEnumerationFails_PropagatesTheException()
    {
        var chatService = Substitute.For<IChatCompletionService>();
        chatService
            .GetStreamingChatMessageContentsAsync(Arg.Any<ChatHistory>(), Arg.Any<PromptExecutionSettings?>(), Arg.Any<Kernel?>(), Arg.Any<CancellationToken>())
            .Returns(FailingStreamingSequence(new InvalidOperationException("stream failed")));

        var client = new SKChatClient(chatService, NullLogger<SKChatClient>.Instance);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            _ = await client.CompleteStreamingAsync([LLMChatMessage.User("hello")]).ToListAsync());

        Assert.That(exception?.Message, Is.EqualTo("stream failed"));
    }

    private static ChatMessageContent CreateChatMessageContent(string? content, string? modelId, IReadOnlyDictionary<string, object?>? metadata = null)
    {
        var message = new ChatMessageContent(AuthorRole.Assistant, content ?? string.Empty)
        {
            ModelId = modelId,
            Metadata = metadata,
        };

        if (content is null)
        {
            message.Content = null;
        }

        return message;
    }

    private static StreamingChatMessageContent CreateStreamingChatMessageContent(string? content, string? modelId)
        => new(AuthorRole.Assistant, content ?? string.Empty)
        {
            ModelId = modelId,
        };

    private static async IAsyncEnumerable<StreamingChatMessageContent> CreateStreamingSequence(params StreamingChatMessageContent[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return chunk;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<StreamingChatMessageContent> FailingStreamingSequence(Exception exception)
    {
        await Task.Yield();
        await Task.FromException(exception);
        yield break;
    }

    private sealed class FakeChatCompletionService : IChatCompletionService
    {
        public IReadOnlyDictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

        public Func<ChatHistory, PromptExecutionSettings?, Kernel?, CancellationToken, Task<IReadOnlyList<ChatMessageContent>>>? GetContentsAsync { get; init; }

        public Func<ChatHistory, PromptExecutionSettings?, Kernel?, CancellationToken, IAsyncEnumerable<StreamingChatMessageContent>>? GetStreamingContentsAsync { get; init; }

        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
            => GetContentsAsync is not null
                ? GetContentsAsync(chatHistory, executionSettings, kernel, cancellationToken)
                : Task.FromResult<IReadOnlyList<ChatMessageContent>>([]);

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
            => GetStreamingContentsAsync is not null
                ? GetStreamingContentsAsync(chatHistory, executionSettings, kernel, cancellationToken)
                : EmptyStreamingSequence();

        private static async IAsyncEnumerable<StreamingChatMessageContent> EmptyStreamingSequence()
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
