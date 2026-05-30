// -----------------------------------------------------------------------------
// File:        AzureOpenAILLMServiceCollectionExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: IServiceCollection extension methods for registering Azure OpenAI-backed LLM services via Semantic Kernel.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Azure.Identity;
using Chishiki.AI.LLM.Abstractions;
using Chishiki.AI.LLM.SemanticKernel.Chat;
using Chishiki.AI.LLM.SemanticKernel.Embedding;
using Chishiki.AI.LLM.SemanticKernel.Providers;
using Chishiki.AI.LLM.SemanticKernel.Rag;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Chishiki.AI.LLM.SemanticKernel;

/// <summary>Provides <see cref="IServiceCollection"/> extension methods for registering Azure OpenAI-backed LLM services.</summary>
public static class AzureOpenAILLMServiceCollectionExtensions
{
    /// <summary>Registers <see cref="ILLMClient"/>, <see cref="ILLMChatClient"/>, <see cref="ILLMEmbeddingClient"/>, and <see cref="ILLMRagClient"/> backed by Azure OpenAI via Semantic Kernel connectors. .</summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configure">Action to configure <see cref="AzureOpenAIOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAzureOpenAILLMClient(
        this IServiceCollection services,
        Action<AzureOpenAIOptions>? configure = null)
    {
        var options = new AzureOpenAIOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        var kernelBuilder = services.AddKernel();

        if (options.AuthMethod == AzureOpenAIAuthMethod.ManagedIdentity)
        {
            var credential = new DefaultAzureCredential();
            kernelBuilder.AddAzureOpenAIChatCompletion(options.ChatModelId, options.Endpoint, credential);
            kernelBuilder.AddAzureOpenAIEmbeddingGenerator(options.EmbeddingModelId, options.Endpoint, credential);
        }
        else
        {
            kernelBuilder.AddAzureOpenAIChatCompletion(options.ChatModelId, options.Endpoint, options.ApiKey!);
            kernelBuilder.AddAzureOpenAIEmbeddingGenerator(options.EmbeddingModelId, options.Endpoint, options.ApiKey!);
        }

        RegisterCoreServices(services, options.ChatModelId, "azure-openai");
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
