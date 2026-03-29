// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using Chishiki.Hub.Contracts;
using Chishiki.Hub.Contracts.Requests;
using ModelContextProtocol.Server;

namespace Chishiki.Hub.Mcp;

/// <summary>
/// MCP tool class that exposes all hub resource operations as callable tools
/// for Model Context Protocol clients.
/// </summary>
[McpServerToolType]
internal sealed class HubMcpTools
{
    private readonly IHubResourceService _service;

    /// <summary>Initializes a new instance of <see cref="HubMcpTools"/>.</summary>
    /// <param name="service">Hub resource service used to fulfil tool requests.</param>
    public HubMcpTools(IHubResourceService service) => _service = service;

    /// <summary>Lists all resources of the requested type, optionally filtered by tag.</summary>
    /// <param name="resourceType">Resource type: template, agent, hook, instruction, skill, or collection.</param>
    /// <param name="tag">Optional tag to filter results.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>Read-only list of matching <see cref="HubResourceDto"/> instances.</returns>
    [McpServerTool(Name = "list_resources"),
     Description("List resources of a given type (template, agent, hook, instruction, skill, collection).")]
    public Task<IReadOnlyList<HubResourceDto>> ListResourcesAsync(
        [Description("Resource type: template, agent, hook, instruction, skill, or collection")]
        string resourceType,
        [Description("Optional tag filter — only resources carrying this tag are returned")]
        string? tag = null,
        CancellationToken cancellationToken = default) =>
        _service.ListResourcesAsync(ParseType(resourceType), tag, cancellationToken);

    /// <summary>Retrieves the full content of the named resource.</summary>
    /// <param name="resourceType">Resource type.</param>
    /// <param name="name">Exact name of the resource to retrieve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The matching <see cref="HubResourceDto"/>, or <see langword="null"/> if not found.</returns>
    [McpServerTool(Name = "get_resource"),
     Description("Retrieve the full content of a named resource.")]
    public Task<HubResourceDto?> GetResourceAsync(
        [Description("Resource type")] string resourceType,
        [Description("Exact name of the resource")] string name,
        CancellationToken cancellationToken = default) =>
        _service.GetResourceAsync(ParseType(resourceType), name, cancellationToken);

    /// <summary>Scaffolds a new resource from a built-in template and registers it in the hub.</summary>
    /// <param name="resourceType">Resource type to create.</param>
    /// <param name="name">Unique resource name used as the file name.</param>
    /// <param name="description">Short human-readable description.</param>
    /// <param name="tags">Comma-separated tags, e.g. "csharp, testing".</param>
    /// <param name="templateName">Optional template name to scaffold from.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The newly created <see cref="HubResourceDto"/>.</returns>
    [McpServerTool(Name = "create_resource"),
     Description("Scaffold a new resource from a built-in template and register it in the hub.")]
    public Task<HubResourceDto> CreateResourceAsync(
        [Description("Resource type")] string resourceType,
        [Description("Unique resource name (used as filename)")] string name,
        [Description("Short human-readable description")] string description,
        [Description("Comma-separated tags, e.g. \"csharp, testing\"")] string? tags = null,
        [Description("Optional template name to scaffold from")] string? templateName = null,
        CancellationToken cancellationToken = default)
    {
        var tagList = (tags ?? string.Empty)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return _service.CreateResourceAsync(
            new CreateResourceRequest(ParseType(resourceType), name, description, tagList, templateName),
            cancellationToken);
    }

    /// <summary>Enables all assets listed in the named collection for the current workspace.</summary>
    /// <param name="collectionName">Name of the collection to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>List of <see cref="HubResourceDto"/> instances that were applied.</returns>
    [McpServerTool(Name = "apply_collection"),
     Description("Enable all assets listed in a named collection for the current workspace.")]
    public Task<IReadOnlyList<HubResourceDto>> ApplyCollectionAsync(
        [Description("Name of the collection to apply")] string collectionName,
        CancellationToken cancellationToken = default) =>
        _service.ApplyCollectionAsync(collectionName, cancellationToken);

    /// <summary>Generates a new project from the named template into the target directory.</summary>
    /// <param name="templateName">Template name to scaffold from.</param>
    /// <param name="targetDirectory">Absolute or relative path to the output directory.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>Absolute path to the generated project directory.</returns>
    [McpServerTool(Name = "scaffold_project"),
     Description("Generate a new project from a named template into a target directory.")]
    public Task<string> ScaffoldProjectAsync(
        [Description("Template name")] string templateName,
        [Description("Absolute or relative path to the output directory")] string targetDirectory,
        CancellationToken cancellationToken = default) =>
        _service.ScaffoldProjectAsync(new ScaffoldProjectRequest(templateName, targetDirectory), cancellationToken);

    /// <summary>Performs full-text search across resource names, descriptions, and tags.</summary>
    /// <param name="query">Free-text query string.</param>
    /// <param name="resourceType">Optional resource type to restrict the search.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>List of matching <see cref="HubResourceDto"/> instances.</returns>
    [McpServerTool(Name = "search_resources"),
     Description("Full-text search across resource names, descriptions, and tags.")]
    public Task<IReadOnlyList<HubResourceDto>> SearchResourcesAsync(
        [Description("Free-text query")] string query,
        [Description("Optional: filter results to a single resource type")] string? resourceType = null,
        CancellationToken cancellationToken = default)
    {
        var type = resourceType is not null ? ParseType(resourceType) : (HubResourceType?)null;
        return _service.SearchResourcesAsync(new SearchResourcesRequest(query, type), cancellationToken);
    }

    /// <summary>Parses a string into a <see cref="HubResourceType"/> enum value.</summary>
    /// <param name="value">Case-insensitive string representation of the resource type.</param>
    /// <returns>The corresponding <see cref="HubResourceType"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> does not match any known resource type.</exception>
    private static HubResourceType ParseType(string value) =>
        Enum.TryParse<HubResourceType>(value, ignoreCase: true, out var result)
            ? result
            : throw new ArgumentException(
                $"Unknown resource type '{value}'. Valid values: " +
                string.Join(", ", Enum.GetNames<HubResourceType>().Select(n => n.ToLowerInvariant())));
}
