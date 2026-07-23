// -----------------------------------------------------------------------------
// File:        Program.cs
// Author:      Piergiorgio Vagnozzi
// Description: Entry point for the Chishiki Agent TUI — wires Spectre.Console.Cli
//              with the DI host and registers all available commands.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Core.Extensions;
using Chishiki.Agent.Tui.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;

// ── Host ──────────────────────────────────────────────────────────────────────
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddChishikiAgent(ctx.Configuration);
        services.AddTransient<ChatCommand>();
    })
    .Build();

await host.StartAsync();

// ── CLI ───────────────────────────────────────────────────────────────────────
var registrar = new TypeRegistrar(host.Services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    _ = config
        .SetApplicationName("chishiki")
        .SetApplicationVersion("1.0.0");

    _ = config.AddCommand<ChatCommand>("chat")
          .WithDescription("Start an interactive chat session with the Chishiki Agent.")
          .WithExample("chat")
          .WithExample("chat", "--model", "ollama:llama3")
          .WithExample("chat", "--model", "openai:gpt-4o", "--no-stream");
});

var exitCode = await app.RunAsync(args);

await host.StopAsync();
return exitCode;

// ── Spectre.Console DI bridge ─────────────────────────────────────────────────

/// <summary>Bridges Microsoft DI with Spectre.Console.Cli's <see cref="ITypeRegistrar"/>.</summary>
internal sealed class TypeRegistrar(IServiceProvider provider) : ITypeRegistrar
{
    /// <inheritdoc/>
    public ITypeResolver Build() => new TypeResolver(provider);

    /// <inheritdoc/>
    public void Register(Type service, Type implementation) { }

    /// <inheritdoc/>
    public void RegisterInstance(Type service, object implementation) { }

    /// <inheritdoc/>
    public void RegisterLazy(Type service, Func<object> factory) { }
}

/// <summary>Resolves Spectre.Console command types from the Microsoft DI container.</summary>
internal sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    /// <inheritdoc/>
    public object? Resolve(Type? type) =>
        type is null ? null : provider.GetService(type) ?? Activator.CreateInstance(type);
}
