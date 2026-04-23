// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

var builder = WebApplication.CreateBuilder(args);

// ── Aspire service defaults (OTel, health checks, service discovery) ─────────
builder.AddServiceDefaults();

// ── OpenAPI ──────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── MCP server — StreamableHTTP transport ────────────────────────────────────
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly();

// ── CORS (required for VS 2026 and browser-based Copilot clients) ─────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.MapDefaultEndpoints();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    _ = app.MapOpenApi();
}

// ── MCP endpoint ──────────────────────────────────────────────────────────────
app.MapMcp("/mcp");

try
{
    app.Logger.AppStarted();
    await app.RunAsync();
    app.Logger.AppShuttingDown();
}
catch (Exception ex)
{
    app.Logger.AppTerminatedUnexpectedly(ex);
    throw;
}

// ── Structured log definitions ────────────────────────────────────────────────
internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Chishiki MCP Host started")]
    public static partial void AppStarted(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Chishiki MCP Host shutting down gracefully")]
    public static partial void AppShuttingDown(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Chishiki MCP Host terminated unexpectedly")]
    public static partial void AppTerminatedUnexpectedly(this ILogger logger, Exception ex);
}
