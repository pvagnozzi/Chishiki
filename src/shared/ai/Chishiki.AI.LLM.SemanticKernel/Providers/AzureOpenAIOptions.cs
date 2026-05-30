// -----------------------------------------------------------------------------
// File:        AzureOpenAIOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for connecting to Azure OpenAI via Semantic Kernel.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.SemanticKernel.Providers;

/// <summary>Enumerates the supported authentication methods for Azure OpenAI.</summary>
public enum AzureOpenAIAuthMethod
{
    /// <summary>Authenticate with an Azure API key.</summary>
    ApiKey,

    /// <summary>Authenticate using Microsoft Entra ID (managed identity or DefaultAzureCredential).</summary>
    ManagedIdentity,
}

/// <summary>Configuration options for connecting to Azure OpenAI via Semantic Kernel.</summary>
public sealed class AzureOpenAIOptions : SKProviderOptions
{
    /// <summary>Gets or sets the Azure OpenAI endpoint URL (e.g. <c>https://my-resource.openai.azure.com</c>).</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Gets or sets the Azure API key. Required when <see cref="AuthMethod"/> is <see cref="AzureOpenAIAuthMethod.ApiKey"/>.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Gets or sets the authentication method. Defaults to <see cref="AzureOpenAIAuthMethod.ApiKey"/>.</summary>
    public AzureOpenAIAuthMethod AuthMethod { get; set; } = AzureOpenAIAuthMethod.ApiKey;

    /// <summary>Initialises a new instance with the recommended Azure OpenAI defaults.</summary>
    public AzureOpenAIOptions()
    {
        ChatModelId = "gpt-4o";
        EmbeddingModelId = "text-embedding-3-small";
    }
}
