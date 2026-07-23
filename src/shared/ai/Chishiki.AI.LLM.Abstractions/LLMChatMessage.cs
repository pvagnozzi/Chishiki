// -----------------------------------------------------------------------------
// File:        ChatMessage.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a single message in an LLM chat conversation, including its role and content.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Represents a single message in an LLM chat conversation, including its role and text content.</summary>
/// <param name="Role">The role of the participant who authored this message.</param>
/// <param name="Content">The text content of the message.</param>
public sealed record LLMChatMessage(LLMChatRole Role, string Content)
{
    /// <summary>Creates a system message with the specified content.</summary>
    /// <param name="content">The system instruction text.</param>
    /// <returns>A new <see cref="LLMChatMessage"/> with <see cref="LLMChatRole.System"/> role.</returns>
    public static LLMChatMessage System(string content) => new(LLMChatRole.System, content);

    /// <summary>Creates a user message with the specified content.</summary>
    /// <param name="content">The user input text.</param>
    /// <returns>A new <see cref="LLMChatMessage"/> with <see cref="LLMChatRole.User"/> role.</returns>
    public static LLMChatMessage User(string content) => new(LLMChatRole.User, content);

    /// <summary>Creates an assistant message with the specified content.</summary>
    /// <param name="content">The assistant-generated text.</param>
    /// <returns>A new <see cref="LLMChatMessage"/> with <see cref="LLMChatRole.Assistant"/> role.</returns>
    public static LLMChatMessage Assistant(string content) => new(LLMChatRole.Assistant, content);
}
