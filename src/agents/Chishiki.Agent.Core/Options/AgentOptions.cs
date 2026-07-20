// -----------------------------------------------------------------------------
// File:        AgentOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Root configuration options for the Chishiki Agent host.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.Core.Options;

/// <summary>Root configuration options for the Chishiki Agent.</summary>
public sealed class AgentOptions
{
    /// <summary>Gets or sets the HTTP port the Web API listens on. Default is 5100.</summary>
    public int Port { get; set; } = 5100;

    /// <summary>Gets or sets the directory from which plugins are loaded. Default is "plugins".</summary>
    public string PluginsDirectory { get; set; } = "plugins";

    /// <summary>Gets or sets model routing configuration.</summary>
    public RoutingOptions Routing { get; set; } = new();

    /// <summary>Gets or sets per-provider configuration keyed by provider ID.</summary>
    public Dictionary<string, ProviderOptions> Providers { get; set; } = [];
}

/// <summary>Configuration options for the model router.</summary>
public sealed class RoutingOptions
{
    /// <summary>Gets or sets the default model alias when no specific model is requested. Default is "local".</summary>
    public string DefaultModel { get; set; } = "local";

    /// <summary>Gets or sets the named route table mapping aliases to provider and model.</summary>
    public Dictionary<string, RouteConfig> Routes { get; set; } = [];

    /// <summary>Gets or sets the ordered fallback provider list used when a primary provider fails.</summary>
    public List<string> Fallback { get; set; } = [];
}

/// <summary>Configuration options for a single AI provider.</summary>
public sealed class ProviderOptions
{
    /// <summary>Gets or sets a value indicating whether this provider is active. Default is true.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets the API key used to authenticate with the provider.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Gets or sets the base URL of the provider API. Used to override the default endpoint.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Gets or sets the Azure OpenAI resource endpoint.</summary>
    public string? Endpoint { get; set; }

    /// <summary>Gets or sets the API version string (Azure OpenAI).</summary>
    public string? ApiVersion { get; set; }
}
