// -----------------------------------------------------------------------------
// File:        CompletionResponse.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents the complete response returned by a provider after a chat-completion request.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>Represents the complete, non-streamed response returned by a provider after a chat-completion request.</summary>
/// <param name="Id">Provider-assigned identifier for this completion.</param>
/// <param name="Model">The model identifier that generated the response.</param>
/// <param name="Provider">The identifier of the provider that processed the request.</param>
/// <param name="Message">The assistant message produced by the model.</param>
/// <param name="PromptTokens">Number of tokens consumed by the input prompt.</param>
/// <param name="CompletionTokens">Number of tokens generated in the completion.</param>
/// <param name="TotalTokens">Total tokens consumed (prompt + completion).</param>
/// <param name="FinishReason">The reason the model stopped generating, e.g. <c>stop</c> or <c>tool_calls</c>.</param>
/// <param name="ToolCalls">Optional list of tool calls requested by the model.</param>
/// <param name="DurationMs">Wall-clock time in milliseconds for the provider to return the response.</param>
public sealed record CompletionResponse(
    string Id,
    string Model,
    string Provider,
    ChatMessage Message,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    string FinishReason,
    IReadOnlyList<ToolCall>? ToolCalls,
    long DurationMs);
