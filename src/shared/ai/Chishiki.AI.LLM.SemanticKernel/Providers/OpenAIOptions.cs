// -----------------------------------------------------------------------------
// File:        OpenAIOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for connecting to the OpenAI API via Semantic Kernel.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.SemanticKernel.Providers;

/// <summary>Configuration options for connecting to the OpenAI API via Semantic Kernel.</summary>
public sealed class OpenAIOptions : SKProviderOptions
{
    /// <summary>Gets or sets the OpenAI API key.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional organisation ID.</summary>
    public string? OrganizationId { get; set; }

    /// <summary>Initialises a new instance with the recommended OpenAI defaults.</summary>
    public OpenAIOptions()
    {
        ChatModelId = "gpt-4o-mini";
        EmbeddingModelId = "text-embedding-3-small";
    }
}
