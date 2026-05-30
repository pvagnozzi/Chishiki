// -----------------------------------------------------------------------------
// File:        OllamaOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for connecting to a local Ollama instance via Semantic Kernel.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.SemanticKernel.Providers;

/// <summary>Configuration options for connecting to a local Ollama instance via Semantic Kernel.</summary>
public sealed class OllamaOptions : SKProviderOptions
{
    /// <summary>Gets or sets the base URL of the Ollama HTTP API. Defaults to <c>http://localhost:11434</c>.</summary>
    public string Endpoint { get; set; } = "http://localhost:11434";

    /// <summary>Initialises a new instance with the recommended Chishiki defaults.</summary>
    public OllamaOptions()
    {
        ChatModelId = "llama3";
        EmbeddingModelId = "nomic-embed-text";
    }
}
