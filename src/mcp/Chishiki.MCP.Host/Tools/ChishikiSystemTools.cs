// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace Chishiki.MCP.Host.Tools;

/// <summary>
/// MCP tools exposing system-level information about the Chishiki MCP server.
/// These are starter tools; domain-specific tools (Hub, RAG, Security) will be
/// added in subsequent development phases.
/// </summary>
[McpServerToolType]
internal sealed partial class ChishikiSystemTools(ILogger<ChishikiSystemTools> logger)
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' invoked")]
    private partial void LogToolInvoked(string toolName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "MCP tool '{ToolName}' completed successfully")]
    private partial void LogToolCompleted(string toolName);

    /// <summary>
    /// Returns version and build metadata for the running Chishiki MCP server.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>JSON object with <c>name</c>, <c>version</c>, <c>framework</c>, and <c>buildTime</c> fields.</returns>
    [McpServerTool(Name = "get_server_info"),
     Description("Returns version and runtime metadata for the Chishiki MCP server.")]
    public Task<string> GetServerInfoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogToolInvoked("get_server_info");

        var assembly = Assembly.GetExecutingAssembly();
        var version  = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                ?.InformationalVersion
                       ?? assembly.GetName().Version?.ToString()
                       ?? "0.0.0";

        var info = new
        {
            name = "Chishiki MCP Server",
            version,
            framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            buildTime = new FileInfo(assembly.Location).LastWriteTimeUtc.ToString("O"),
        };

        LogToolCompleted("get_server_info");
        return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(info));
    }

    /// <summary>
    /// Lists the MCP tool groups that this server exposes, with their implementation status.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>JSON array of tool group descriptors.</returns>
    [McpServerTool(Name = "list_capabilities"),
     Description("Lists the tool groups this Chishiki MCP server exposes and their current implementation status.")]
    public Task<string> ListCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogToolInvoked("list_capabilities");

        var capabilities = new[]
        {
            new { group = "system",   status = "available", description = "Server metadata and health tools" },
            new { group = "hub",      status = "planned",   description = "GitHub Copilot asset management — agents, skills, instructions, collections" },
            new { group = "rag",      status = "planned",   description = "Semantic search and AI-synthesised answers over the ingested knowledge base" },
            new { group = "security", status = "planned",   description = "SAST/SCA scan results and exploit-test generation" },
            new { group = "ingest",   status = "planned",   description = "Trigger and monitor data ingestion from emails, Git, documents, and projects" },
        };

        LogToolCompleted("list_capabilities");
        return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(capabilities));
    }
}
