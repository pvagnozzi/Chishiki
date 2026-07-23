// -----------------------------------------------------------------------------
// File:        SKProviderOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base configuration options shared by all Semantic Kernel LLM provider implementations.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.SemanticKernel.Providers;

/// <summary>Base configuration options shared by all Semantic Kernel LLM provider implementations.</summary>
public abstract class SKProviderOptions
{
    /// <summary>Gets or sets the default chat model identifier.</summary>
    public string ChatModelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the default embedding model identifier.</summary>
    public string EmbeddingModelId { get; set; } = string.Empty;
}
