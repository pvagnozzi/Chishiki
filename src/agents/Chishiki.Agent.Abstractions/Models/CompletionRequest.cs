// -----------------------------------------------------------------------------
// File:        CompletionRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Encapsulates all parameters for a chat-completion request sent to a provider.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>Encapsulates all parameters required to execute a chat-completion request against a provider.</summary>
/// <param name="Model">The model identifier to use for this request.</param>
/// <param name="Messages">The ordered list of messages forming the conversation history.</param>
/// <param name="MaxTokens">Maximum number of tokens to generate. <c>null</c> uses the provider default.</param>
/// <param name="Temperature">Sampling temperature controlling output randomness. Range [0, 2]. <c>null</c> uses the provider default.</param>
/// <param name="TopP">Nucleus-sampling probability mass. Range (0, 1]. <c>null</c> uses the provider default.</param>
/// <param name="Stream">When <c>true</c> the provider should stream the response as a sequence of chunks.</param>
/// <param name="Tools">Optional list of tool definitions the model may invoke.</param>
/// <param name="SystemPrompt">Optional system-level instruction prepended to the conversation.</param>
/// <remarks>
/// <see cref="RequestId"/> defaults to a new <see cref="Guid"/> and can be overridden via <c>with { RequestId = ... }</c>
/// to correlate requests across distributed components.
/// </remarks>
public sealed record CompletionRequest(
    string Model,
    IReadOnlyList<ChatMessage> Messages,
    int? MaxTokens = null,
    float? Temperature = null,
    float? TopP = null,
    bool Stream = false,
    IReadOnlyList<ToolCall>? Tools = null,
    string? SystemPrompt = null)
{
    /// <summary>Gets the unique identifier for this request. Defaults to a freshly generated <see cref="Guid"/>.</summary>
    public Guid RequestId { get; init; } = Guid.NewGuid();
}
