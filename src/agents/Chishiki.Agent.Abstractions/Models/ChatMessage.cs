// -----------------------------------------------------------------------------
// File:        ChatMessage.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a single message in an agent chat conversation.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Agent.Abstractions.Models;

/// <summary>Represents a single message in an agent chat conversation.</summary>
/// <param name="Role">The role of the participant who authored this message.</param>
/// <param name="Content">The text content of the message.</param>
/// <param name="Name">Optional display name of the participant. Useful for multi-agent scenarios.</param>
/// <param name="ToolCallId">Optional tool-call identifier when the role is <see cref="ChatRole.Tool"/>.</param>
public sealed record ChatMessage(
    ChatRole Role,
    string Content,
    string? Name = null,
    string? ToolCallId = null)
{
    #region Factory Methods

    /// <summary>Creates a system message with the specified content.</summary>
    /// <param name="content">The system instruction text.</param>
    /// <returns>A new <see cref="ChatMessage"/> with <see cref="ChatRole.System"/> role.</returns>
    public static ChatMessage System(string content) => new(ChatRole.System, content);

    /// <summary>Creates a user message with the specified content.</summary>
    /// <param name="content">The user input text.</param>
    /// <returns>A new <see cref="ChatMessage"/> with <see cref="ChatRole.User"/> role.</returns>
    public static ChatMessage User(string content) => new(ChatRole.User, content);

    /// <summary>Creates an assistant message with the specified content.</summary>
    /// <param name="content">The assistant-generated text.</param>
    /// <returns>A new <see cref="ChatMessage"/> with <see cref="ChatRole.Assistant"/> role.</returns>
    public static ChatMessage Assistant(string content) => new(ChatRole.Assistant, content);

    /// <summary>Creates a tool-result message with the specified content and tool call identifier.</summary>
    /// <param name="content">The tool output text.</param>
    /// <param name="toolCallId">The identifier of the originating tool call.</param>
    /// <returns>A new <see cref="ChatMessage"/> with <see cref="ChatRole.Tool"/> role.</returns>
    public static ChatMessage Tool(string content, string toolCallId) => new(ChatRole.Tool, content, ToolCallId: toolCallId);

    #endregion
}
