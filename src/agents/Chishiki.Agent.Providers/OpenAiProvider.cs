// -----------------------------------------------------------------------------
// File:        OpenAiProvider.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenAI-compatible provider (also used for OpenRouter).
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Chishiki.Agent.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Chishiki.Agent.Providers;

/// <summary>Provider adapter for OpenAI and OpenAI-compatible APIs (including OpenRouter).</summary>
public sealed partial class OpenAiProvider : HttpProviderBase
{
    #region Fields

    private readonly string _id;
    private readonly string _displayName;

    #endregion

    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="OpenAiProvider"/> class.</summary>
    /// <param name="http">Preconfigured HTTP client (BaseAddress and Authorization set by factory).</param>
    /// <param name="id">The logical provider ID (e.g. "openai" or "openrouter").</param>
    /// <param name="displayName">Human-readable display name.</param>
    /// <param name="logger">Logger instance.</param>
    public OpenAiProvider(HttpClient http, string id, string displayName, ILogger<OpenAiProvider> logger)
        : base(http, logger)
    {
        _id = id;
        _displayName = displayName;
    }

    #endregion

    #region IProvider

    /// <inheritdoc/>
    public override string Id => _id;

    /// <inheritdoc/>
    public override string DisplayName => _displayName;

    /// <inheritdoc/>
    public override async Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync("models", cancellationToken);
        var list = await FromJsonAsync<OpenAiModelList>(response, cancellationToken);
        return list.Data.Select(m => new ModelInfo(m.Id, m.Id, _id, 128_000, 4096, true, false, 0, 0)).ToList();
    }

    /// <inheritdoc/>
    public override async Task<CompletionResponse> CompleteAsync(
        CompletionRequest request, CancellationToken cancellationToken = default)
    {
        LogSendingRequest(Logger, _id, request.Model, request.Messages.Count);
        var body = BuildRequestBody(request, stream: false);
        using var content = ToJson(body);
        var httpResp = await Http.PostAsync("chat/completions", content, cancellationToken);
        var (result, ms) = await MeasureAsync(() => FromJsonAsync<OpenAiChatResponse>(httpResp, cancellationToken));

        var choice = result.Choices.FirstOrDefault();
        LogCompletion(Logger, _id, result.Usage?.TotalTokens ?? 0, ms);

        return new CompletionResponse(
            result.Id,
            result.Model ?? request.Model,
            _id,
            new ChatMessage(ChatRole.Assistant, choice?.Message?.Content ?? string.Empty),
            result.Usage?.PromptTokens ?? 0,
            result.Usage?.CompletionTokens ?? 0,
            result.Usage?.TotalTokens ?? 0,
            choice?.FinishReason ?? "stop",
            null,
            (long)ms);
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<CompletionChunk> StreamAsync(
        CompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var body = BuildRequestBody(request, stream: true);
        using var content = ToJson(body);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions") { Content = content };
        using var httpResp = await Http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        await foreach (var payload in ReadSseAsync(httpResp, cancellationToken))
        {
            OpenAiStreamChunk? chunk;
            try { chunk = JsonSerializer.Deserialize<OpenAiStreamChunk>(payload); }
            catch { continue; }

            if (chunk is null) continue;
            var delta = chunk.Choices.FirstOrDefault()?.Delta?.Content;
            var finish = chunk.Choices.FirstOrDefault()?.FinishReason;
            var isFinished = !string.IsNullOrEmpty(finish);
            yield return new CompletionChunk(chunk.Id, chunk.Model ?? request.Model, delta, isFinished, finish);
        }
    }

    #endregion

    #region Private Helpers

    private static object BuildRequestBody(CompletionRequest request, bool stream) =>
        new
        {
            model = request.Model,
            messages = request.Messages.Select(m => new { role = m.Role.ToString().ToLowerInvariant(), content = m.Content }),
            max_tokens = request.MaxTokens,
            temperature = request.Temperature,
            top_p = request.TopP,
            stream,
        };

    #endregion

    #region Wire DTOs

    private sealed record OpenAiModelList([property: JsonPropertyName("data")] List<OpenAiModelItem> Data);
    private sealed record OpenAiModelItem([property: JsonPropertyName("id")] string Id);
    private sealed record OpenAiChatResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("model")] string? Model,
        [property: JsonPropertyName("choices")] List<OpenAiChoice> Choices,
        [property: JsonPropertyName("usage")] OpenAiUsage? Usage);
    private sealed record OpenAiChoice(
        [property: JsonPropertyName("message")] OpenAiMessage? Message,
        [property: JsonPropertyName("finish_reason")] string? FinishReason);
    private sealed record OpenAiMessage(
        [property: JsonPropertyName("content")] string Content);
    private sealed record OpenAiUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
        [property: JsonPropertyName("total_tokens")] int TotalTokens);
    private sealed record OpenAiStreamChunk(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("model")] string? Model,
        [property: JsonPropertyName("choices")] List<OpenAiStreamChoice> Choices);
    private sealed record OpenAiStreamChoice(
        [property: JsonPropertyName("delta")] OpenAiStreamDelta? Delta,
        [property: JsonPropertyName("finish_reason")] string? FinishReason);
    private sealed record OpenAiStreamDelta(
        [property: JsonPropertyName("content")] string? Content);

    #endregion
}
