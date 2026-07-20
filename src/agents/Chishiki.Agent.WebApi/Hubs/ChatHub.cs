// -----------------------------------------------------------------------------
// File:        ChatHub.cs
// Author:      Piergiorgio Vagnozzi
// Description: SignalR hub for real-time streaming chat completions.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chishiki.Agent.WebApi.Hubs;

/// <summary>SignalR hub that streams chat completion chunks to connected web clients.</summary>
public sealed class ChatHub : Hub
{
    private readonly IOrchestrator _orchestrator;

    /// <summary>Initializes a new instance of the <see cref="ChatHub"/> class.</summary>
    /// <param name="orchestrator">The agent orchestrator used to generate completions.</param>
    public ChatHub(IOrchestrator orchestrator) => _orchestrator = orchestrator;

    /// <summary>
    /// Accepts a chat request from the client, streams completion chunks back via
    /// <c>ReceiveChunk</c>, and sends a final <c>StreamEnd</c> signal.
    /// </summary>
    /// <param name="model">Model alias to use for this request.</param>
    /// <param name="messages">Ordered list of conversation messages.</param>
    /// <param name="cancellationToken">Cancellation token from the SignalR infrastructure.</param>
    public async Task StreamChat(
        string model,
        IEnumerable<ChatMessage> messages,
        CancellationToken cancellationToken)
    {
        var request = new CompletionRequest(model, [..messages], Stream: true);

        await foreach (var chunk in _orchestrator.StreamAsync(request, cancellationToken))
        {
            await Clients.Caller.SendAsync("ReceiveChunk", chunk, cancellationToken);
            if (chunk.IsFinished) break;
        }

        await Clients.Caller.SendAsync("StreamEnd", cancellationToken);
    }
}
