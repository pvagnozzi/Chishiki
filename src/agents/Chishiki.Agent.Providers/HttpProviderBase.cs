// -----------------------------------------------------------------------------
// File:        HttpProviderBase.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for HTTP-based AI providers with SSE streaming support.
// Created:     2026-06-28
// Modified:    2026-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace Chishiki.Agent.Providers;

/// <summary>Abstract base class for HTTP-based AI providers, providing JSON serialization and SSE streaming helpers.</summary>
public abstract partial class HttpProviderBase : IProvider
{
    #region Fields

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private bool _disposed;

    #endregion

    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="HttpProviderBase"/> class.</summary>
    /// <param name="http">Pre-configured HTTP client for this provider.</param>
    /// <param name="logger">Logger instance.</param>
    protected HttpProviderBase(HttpClient http, ILogger logger)
    {
        Http = http;
        Logger = logger;
    }

    #endregion

    #region IProvider

    /// <inheritdoc/>
    public abstract string Id { get; }

    /// <inheritdoc/>
    public abstract string DisplayName { get; }

    /// <inheritdoc/>
    public virtual bool IsAvailable => !string.IsNullOrWhiteSpace(Http.BaseAddress?.ToString());

    /// <inheritdoc/>
    public abstract Task<IReadOnlyList<ModelInfo>> ListModelsAsync(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<CompletionResponse> CompleteAsync(CompletionRequest request, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract IAsyncEnumerable<CompletionChunk> StreamAsync(CompletionRequest request, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            Http.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    #endregion

    #region Properties

    /// <summary>Gets the configured HTTP client used by the provider.</summary>
    protected HttpClient Http { get; }

    /// <summary>Gets the logger used by the provider.</summary>
    protected ILogger Logger { get; }

    #endregion

    #region Protected Helpers

    /// <summary>Serializes an object to a <see cref="StringContent"/> with <c>application/json</c> media type.</summary>
    protected static StringContent ToJson<T>(T value) =>
        new(JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8, "application/json");

    /// <summary>Deserializes a JSON response body to <typeparamref name="T"/>.</summary>
    protected static async Task<T> FromJsonAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync(ct);
        return (await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, ct))!;
    }

    /// <summary>
    /// Reads an SSE stream and yields each data payload string.
    /// Stops when the stream ends or a <c>data: [DONE]</c> line is received.
    /// </summary>
    protected static async IAsyncEnumerable<string> ReadSseAsync(
        HttpResponseMessage response,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            if (!line.StartsWith("data: ", StringComparison.Ordinal)) continue;
            var data = line[6..];
            if (data == "[DONE]") break;
            yield return data;
        }
    }

    /// <summary>Measures execution duration and logs the result.</summary>
    protected static async Task<(T Result, long DurationMs)> MeasureAsync<T>(Func<Task<T>> action)
    {
        var sw = Stopwatch.StartNew();
        var result = await action();
        return (result, sw.ElapsedMilliseconds);
    }

    #endregion

    #region Logging

    /// <summary>Logs that a completion request is being sent to a provider.</summary>
    [LoggerMessage(EventId = 3000, Level = LogLevel.Debug,
        Message = "Sending completion request to {ProviderId}: model='{Model}' messages={MessageCount}.")]
    protected static partial void LogSendingRequest(ILogger logger, string providerId, string model, int messageCount);

    /// <summary>Logs that a provider request failed.</summary>
    [LoggerMessage(EventId = 3001, Level = LogLevel.Warning,
        Message = "Provider {ProviderId} request failed: {StatusCode}.")]
    protected static partial void LogRequestFailed(ILogger logger, string providerId, int statusCode, Exception exception);

    /// <summary>Logs the completion result returned by a provider.</summary>
    [LoggerMessage(EventId = 3002, Level = LogLevel.Debug,
        Message = "Provider {ProviderId} returned {TokenCount} tokens in {DurationMs}ms.")]
    protected static partial void LogCompletion(ILogger logger, string providerId, int tokenCount, long durationMs);

    #endregion
}
