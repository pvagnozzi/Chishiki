// -----------------------------------------------------------------------------
// File:        CompletionEndpoints.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenAI-compatible /v1/chat/completions and /v1/models endpoints.
// Created:     2026-06-28
// Modified:    2026-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Text;
using System.Text.Json;
using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.WebApi.Endpoints;

/// <summary>Registers OpenAI-compatible API endpoints on the web application.</summary>
public static class CompletionEndpoints
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false,
    };

    /// <summary>Maps all agent API routes onto the provided <see cref="WebApplication"/>.</summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The same application for chaining.</returns>
    public static WebApplication MapAgentEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/v1");

        // ── GET /v1/models ────────────────────────────────────────────────
        api.MapGet("/models", async (IOrchestrator orchestrator, CancellationToken ct) =>
        {
            var models = await orchestrator.ListModelsAsync(ct);
            return Results.Ok(new
            {
                Object = "list",
                Data = models.Select(m => new
                {
                    id = m.Id,
                    @object = "model",
                    owned_by = m.Provider,
                    context_window = m.ContextWindow,
                }),
            });
        })
        .WithName("ListModels")
        .WithSummary("Lists all available models across configured providers.");

        // ── POST /v1/chat/completions ──────────────────────────────────────
        api.MapPost("/chat/completions", async (
            HttpContext http,
            IOrchestrator orchestrator,
            CompletionRequest request,
            CancellationToken ct) =>
        {
            if (request.Stream)
            {
                http.Response.Headers.ContentType = "text/event-stream";
                http.Response.Headers.CacheControl = "no-cache";

                await foreach (var chunk in orchestrator.StreamAsync(request, ct))
                {
                    var json = JsonSerializer.Serialize(chunk, JsonOpts);
                    await http.Response.WriteAsync($"data: {json}\n\n", ct);
                    await http.Response.Body.FlushAsync(ct);

                    if (chunk.IsFinished)
                    {
                        await http.Response.WriteAsync("data: [DONE]\n\n", ct);
                        break;
                    }
                }
            }
            else
            {
                var response = await orchestrator.CompleteAsync(request, ct);
                return Results.Ok(response);
            }

            return Results.Empty;
        })
        .WithName("ChatCompletions")
        .WithSummary("Creates a chat completion. Supports streaming via SSE when stream=true.");

        // ── GET /v1/plugins ────────────────────────────────────────────────
        api.MapGet("/plugins", (IOrchestrator orchestrator) =>
            Results.Ok(orchestrator.Plugins.Select(p => new
            {
                p.Id,
                p.DisplayName,
                p.Version,
                p.Description,
                Capabilities = p.Capabilities.Select(c => new { c.Name, c.Description }),
            })))
        .WithName("ListPlugins")
        .WithSummary("Lists all currently loaded plugins and their capabilities.");

        // ── POST /v1/plugins/{id}/capabilities/{cap} ──────────────────────
        api.MapPost("/plugins/{pluginId}/capabilities/{capabilityName}",
            async (string pluginId, string capabilityName, IOrchestrator orchestrator,
                   CapabilityRequest request, CancellationToken ct) =>
            {
                var result = await orchestrator.ExecuteCapabilityAsync(pluginId, request, ct);
                return result.Success ? Results.Ok(result) : Results.UnprocessableEntity(result);
            })
        .WithName("ExecuteCapability")
        .WithSummary("Executes a named capability on a loaded plugin.");

        // ── GET /health ────────────────────────────────────────────────────
        app.MapGet("/health", (IOrchestrator orchestrator) =>
            Results.Ok(new
            {
                Status = "healthy",
                Providers = orchestrator.Providers.Select(p => new { p.Id, p.DisplayName, p.IsAvailable }),
                Plugins = orchestrator.Plugins.Count,
                Timestamp = DateTimeOffset.UtcNow,
            }))
        .WithName("Health")
        .WithSummary("Returns the health status of the agent and all configured providers.");

        return app;
    }
}
