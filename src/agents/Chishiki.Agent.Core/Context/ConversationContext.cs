// -----------------------------------------------------------------------------
// File:        ConversationContext.cs
// Author:      Piergiorgio Vagnozzi
// Description: Manages multi-turn conversation history with automatic trimming.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.Core.Context;

/// <summary>Manages the history of a single conversation with automatic context-window trimming.</summary>
public sealed class ConversationContext
{
    #region Fields

    private readonly List<ChatMessage> _history = [];

    private readonly Lock _lock = new();

    #endregion

    #region Properties

    /// <summary>Gets the unique identifier for this conversation.</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>Gets or sets the maximum number of messages retained before oldest are dropped. Default is 100.</summary>
    public int MaxMessages { get; set; } = 100;

    /// <summary>Gets the current message count.</summary>
    public int Count { get { lock (_lock) { return _history.Count; } } }

    #endregion

    #region Public Methods

    /// <summary>Appends a message to the conversation history.</summary>
    /// <param name="message">The message to append.</param>
    public void AddMessage(ChatMessage message)
    {
        lock (_lock)
        {
            _history.Add(message);

            // Trim to MaxMessages, preserving the system message if present
            while (_history.Count > MaxMessages)
            {
                var removeAt = _history.Count > 1 && _history[0].Role == ChatRole.System ? 1 : 0;
                _history.RemoveAt(removeAt);
            }
        }
    }

    /// <summary>Clears all messages from the history.</summary>
    public void Clear()
    {
        lock (_lock) { _history.Clear(); }
    }

    /// <summary>
    /// Returns a trimmed view of the history that fits within the estimated token budget.
    /// Uses a simple heuristic of 4 characters ≈ 1 token.
    /// </summary>
    /// <param name="maxTokenEstimate">Maximum estimated tokens. Default is 4000.</param>
    /// <returns>A read-only snapshot of messages that fit the budget, newest first preference.</returns>
    public IReadOnlyList<ChatMessage> GetTrimmedHistory(int maxTokenEstimate = 4000)
    {
        lock (_lock)
        {
            var snapshot = _history.ToList();
            if (snapshot.Count == 0) return [];

            // Always include the system message if first
            var startIdx = 0;
            ChatMessage? system = null;
            if (snapshot[0].Role == ChatRole.System)
            {
                system = snapshot[0];
                startIdx = 1;
                maxTokenEstimate -= EstimateTokens(system.Content);
            }

            // Fill from newest to oldest
            var selected = new List<ChatMessage>();
            var remaining = maxTokenEstimate;
            for (var i = snapshot.Count - 1; i >= startIdx; i--)
            {
                var msg = snapshot[i];
                var est = EstimateTokens(msg.Content);
                if (remaining - est < 0) break;
                selected.Insert(0, msg);
                remaining -= est;
            }

            if (system is not null) selected.Insert(0, system);
            return selected.AsReadOnly();
        }
    }

    /// <summary>Returns a read-only snapshot of the full history.</summary>
    public IReadOnlyList<ChatMessage> GetHistory()
    {
        lock (_lock)
        {
            return _history.AsReadOnly();
        }
    }

    #endregion

    #region Private Helpers

    private static int EstimateTokens(string text) => (text?.Length ?? 0) / 4;

    #endregion
}
