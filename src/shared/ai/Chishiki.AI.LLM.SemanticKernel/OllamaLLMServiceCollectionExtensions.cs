// -----------------------------------------------------------------------------
// File:        OllamaLLMServiceCollectionExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: IServiceCollection extension methods for registering Ollama-backed LLM services via Semantic Kernel.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.AI.LLM.Abstractions;
using Chishiki.AI.LLM.SemanticKernel.Chat;
using Chishiki.AI.LLM.SemanticKernel.Embedding;
using Chishiki.AI.LLM.SemanticKernel.Providers;
using Chishiki.AI.LLM.SemanticKernel.Rag;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;

namespace Chishiki.AI.LLM.SemanticKernel;

/// <summary>Provides <see cref="IServiceCollection"/> extension methods for registering Ollama-backed LLM services.</summary>
public static class OllamaLLMServiceCollectionExtensions
{
    /// <summary>Registers <see cref="ILLMClient"/>, <see cref="ILLMChatClient"/>, <see cref="ILLMEmbeddingClient"/>, and <see cref="ILLMRagClient"/> backed by Ollama via Semantic Kernel connectors. .</summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">Action to configure <see cref="OllamaOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddOllamaLLMClient(
        this IServiceCollection services,
        Action<OllamaOptions>? configure = null)
    {
        var options = new OllamaOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        var endpoint = new Uri(options.Endpoint);
        var kernelBuilder = services.AddKernel();

#pragma warning disable SKEXP0070
        kernelBuilder.AddOllamaChatCompletion(options.ChatModelId, endpoint);
        kernelBuilder.AddOllamaEmbeddingGenerator(options.EmbeddingModelId, endpoint);
#pragma warning restore SKEXP0070

        RegisterCoreServices(services, options.ChatModelId, "ollama");
        return services;
    }

    private static void RegisterCoreServices(IServiceCollection services, string defaultModelId, string providerName)
    {
        services.AddTransient<SKChatClient>();
        services.AddTransient<SKEmbeddingClient>();
        services.AddTransient<SKRagClient>();

        services.AddTransient<SKLLMClient>(sp => new SKLLMClient(
            sp.GetRequiredService<SKChatClient>(),
            sp.GetRequiredService<SKEmbeddingClient>(),
            sp.GetRequiredService<SKRagClient>(),
            defaultModelId,
            providerName,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SKLLMClient>>()));

        services.AddTransient<ILLMClient>(sp => sp.GetRequiredService<SKLLMClient>());
        services.AddTransient<ILLMChatClient>(sp => sp.GetRequiredService<SKLLMClient>());
        services.AddTransient<ILLMEmbeddingClient>(sp => sp.GetRequiredService<SKLLMClient>());
        services.AddTransient<ILLMRagClient>(sp => sp.GetRequiredService<SKLLMClient>());
    }
}
