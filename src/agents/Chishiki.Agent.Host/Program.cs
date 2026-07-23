// -----------------------------------------------------------------------------
// File:        Program.cs
// Author:      Piergiorgio Vagnozzi
// Description: Self-contained host entry point composing Web API and TUI.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Core.Extensions;
using Chishiki.Agent.Core.Options;
using Chishiki.Agent.Core.Plugins;
using Chishiki.Agent.Tui.Commands;
using Chishiki.Agent.WebApi.Endpoints;
using Chishiki.Agent.WebApi.Hubs;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

// ── Parse CLI flags ───────────────────────────────────────────────────────────
#pragma warning disable format
var headless = args.Any(a => a is "--headless" or "--web-only");
var tuiOnly  = args.Any(a => a is "--tui-only");
var port     = ParsePort(args);
#pragma warning restore format

// ── Build web host ────────────────────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddChishikiAgent(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapOpenApi();
app.MapHub<ChatHub>("/ws/chat");
app.MapAgentEndpoints();

// ── Load plugins ──────────────────────────────────────────────────────────────
var pluginLoader = app.Services.GetRequiredService<PluginLoader>();
var agentOpts    = app.Services.GetRequiredService<IOptions<AgentOptions>>().Value;

if (tuiOnly)
{
    // TUI-only: no web server, run CLI directly
    _ = await RunTuiAsync(app.Services, args);
    return 0;
}

if (headless)
{
    // Web-only: run API server, no TUI
    AnsiConsole.MarkupLine($"[bold blue]Chishiki Agent[/] (headless) ▶ [cyan]http://localhost:{port}[/]");
    await pluginLoader.LoadFromDirectoryAsync(agentOpts.PluginsDirectory);
    await app.RunAsync();
    return 0;
}

// ── Default: Web API in background + TUI in foreground ───────────────────────
AnsiConsole.MarkupLine($"[bold blue]Chishiki Agent[/] ▶ [cyan]http://localhost:{port}[/]  |  Ctrl+C to exit");

var webTask = Task.Run(async () =>
{
    await pluginLoader.LoadFromDirectoryAsync(agentOpts.PluginsDirectory);
    await app.StartAsync();
    await app.WaitForShutdownAsync();
});

await Task.Delay(800); // let web server bind before TUI starts

await RunTuiAsync(app.Services, ["chat"]);
await app.StopAsync();
await webTask;
return 0;

// ── Local functions ───────────────────────────────────────────────────────────
static async Task<int> RunTuiAsync(IServiceProvider services, string[] cliArgs)
{
    var cli = new CommandApp(new TypeRegistrar(services));
    cli.Configure(cfg => cfg
        .SetApplicationName("chishiki-agent")
        .AddCommand<ChatCommand>("chat")
           .WithDescription("Start an interactive chat session."));
    return await cli.RunAsync(cliArgs);
}

static int ParsePort(string[] args)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (args[i] is "--port" or "-p" && int.TryParse(args[i + 1], out var p))
            return p;
    }
    return 5100;
}

// ── DI glue for Spectre.Console.Cli ──────────────────────────────────────────
internal sealed class TypeRegistrar(IServiceProvider provider) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(provider);
    public void Register(Type service, Type implementation) { }
    public void RegisterInstance(Type service, object implementation) { }
    public void RegisterLazy(Type service, Func<object> factory) { }
}

internal sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object? Resolve(Type? type) =>
        type is null ? null : provider.GetService(type) ?? Activator.CreateInstance(type);
}
