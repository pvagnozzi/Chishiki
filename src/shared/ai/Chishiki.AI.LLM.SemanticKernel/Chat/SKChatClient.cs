// -----------------------------------------------------------------------------
// File:        SKChatClient.cs
// Author:      Piergiorgio Vagnozzi
// Description: ILLMChatClient implementation backed by a Semantic Kernel IChatCompletionService.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Globalization;
using Chishiki.AI.LLM.Abstractions;
using Chishiki.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Chishiki.AI.LLM.SemanticKernel.Chat;

/// <summary>Implements <see cref="ILLMChatClient"/> backed by a Semantic Kernel <see cref="IChatCompletionService"/>.</summary>
/// <param name="chatService">The Semantic Kernel chat completion service.</param>
/// <param name="logger">Logger instance.</param>
public sealed partial class SKChatClient(
    IChatCompletionService chatService,
    ILogger<SKChatClient> logger) : Service(logger), ILLMChatClient
{
    /// <inheritdoc/>
    public async Task<LLMChatResponse> CompleteAsync(
        IReadOnlyList<LLMChatMessage> messages,
        LLMChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogChatStarted(messages.Count);

        var history = BuildHistory(messages);
        var settings = BuildSettings(options);

        try
        {
            var result = await chatService.GetChatMessageContentAsync(history, settings, cancellationToken: cancellationToken);
            var response = new LLMChatResponse(
                Content: result.Content ?? string.Empty,
                ModelId: result.ModelId,
                FinishReason: result.Metadata?.TryGetValue("FinishReason", out var fr) == true ? fr?.ToString() : null,
                Usage: ExtractUsage(result.Metadata));

            LogChatCompleted(response.Content.Length);
            return response;
        }
        catch (Exception ex)
        {
            LogChatFailed(ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<LLMStreamingChatChunk> CompleteStreamingAsync(
        IReadOnlyList<LLMChatMessage> messages,
        LLMChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogStreamingStarted(messages.Count);

        var history = BuildHistory(messages);
        var settings = BuildSettings(options);
        var chunkCount = 0;

        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(history, settings, cancellationToken: cancellationToken))
        {
            chunkCount++;
            LogChunkReceived(chunkCount);
            yield return new LLMStreamingChatChunk(chunk.Content ?? string.Empty, chunk.ModelId);
        }

        LogStreamingCompleted(chunkCount);
        yield return new LLMStreamingChatChunk(string.Empty, IsFinal: true);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    internal static ChatHistory BuildHistory(IReadOnlyList<LLMChatMessage> messages)
    {
        var history = new ChatHistory();
        foreach (var m in messages)
        {
            switch (m.Role)
            {
                case LLMChatRole.System:
                    history.AddSystemMessage(m.Content);
                    break;
                case LLMChatRole.User:
                    history.AddUserMessage(m.Content);
                    break;
                case LLMChatRole.Assistant:
                    history.AddAssistantMessage(m.Content);
                    break;
                case LLMChatRole.Tool:
                    history.Add(new ChatMessageContent(AuthorRole.Tool, m.Content));
                    break;
            }
        }
        return history;
    }

    internal static PromptExecutionSettings? BuildSettings(LLMChatOptions? options) =>
        options is null
            ? null
            : new PromptExecutionSettings
            {
                ModelId = options.ModelId,
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = options.Temperature ?? 0,
                    ["top_p"] = options.TopP ?? 0,
                    ["max_tokens"] = options.MaxTokens ?? 0,
                    ["stop"] = options.StopSequences ?? []
                }
            };

    private static LLMTokenUsage? ExtractUsage(IReadOnlyDictionary<string, object?>? metadata)
    {
        if (metadata is null)
        {
            return null;
        }

        var prompt = metadata.TryGetValue("PromptTokenCount", out var p) ? Convert.ToInt32(p, CultureInfo.InvariantCulture) : 0;
        var completion = metadata.TryGetValue("CompletionTokenCount", out var c) ? Convert.ToInt32(c, CultureInfo.InvariantCulture) : 0;
        return prompt == 0 && completion == 0 ? null : new LLMTokenUsage(prompt, completion, prompt + completion);
    }

    #region Log messages
    [LoggerMessage(Level = LogLevel.Debug, Message = "Starting chat completion with {MessageCount} messages")]
    private partial void LogChatStarted(int messageCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Chat completion finished, response length {ContentLength} chars")]
    private partial void LogChatCompleted(int contentLength);

    [LoggerMessage(Level = LogLevel.Error, Message = "Chat completion failed")]
    private partial void LogChatFailed(Exception ex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Starting streaming chat with {MessageCount} messages")]
    private partial void LogStreamingStarted(int messageCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Streaming chunk {ChunkIndex} received")]
    private partial void LogChunkReceived(int chunkIndex);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Streaming chat completed after {TotalChunks} chunks")]
    private partial void LogStreamingCompleted(int totalChunks);
    #endregion
}
