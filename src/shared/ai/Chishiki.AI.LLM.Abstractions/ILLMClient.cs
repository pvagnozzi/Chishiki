// -----------------------------------------------------------------------------
// File:        ILLMClient.cs
// Author:      Piergiorgio Vagnozzi
// Description: Unified LLM client interface combining chat completion, embedding, and RAG capabilities.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Unified LLM client interface combining chat completion, embedding, and RAG capabilities.</summary>
public interface ILLMClient : ILLMChatClient, ILLMEmbeddingClient, ILLMRagClient
{
    /// <summary>Gets the identifier of the default model used by this client.</summary>
    string DefaultModelId { get; }

    /// <summary>Gets the name of the provider backing this client (e.g. <c>"ollama"</c>, <c>"openai"</c>).</summary>
    string ProviderName { get; }
}
