// -----------------------------------------------------------------------------
// File:        ServiceCollectionExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: DI registration extensions for the Chishiki Agent Core.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Core.Context;
using Chishiki.Agent.Core.Options;
using Chishiki.Agent.Core.Orchestration;
using Chishiki.Agent.Core.Plugins;
using Chishiki.Agent.Core.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chishiki.Agent.Core.Extensions;

/// <summary>Extension methods for registering Chishiki Agent Core services with the DI container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all core Chishiki Agent services: options, router, plugin loader, orchestrator, and conversation context.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration from which <c>Agent</c> section is read.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddChishikiAgent(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AgentOptions>(configuration.GetSection("Agent"));
        services.AddSingleton<IModelRouter, ModelRouter>();
        services.AddSingleton<PluginLoader>();
        services.AddSingleton<IOrchestrator, AgentOrchestrator>();
        services.AddScoped<ConversationContext>();
        return services;
    }
}
