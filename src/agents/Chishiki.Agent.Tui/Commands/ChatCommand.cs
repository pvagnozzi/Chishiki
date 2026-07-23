// -----------------------------------------------------------------------------
// File:        ChatCommand.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interactive chat REPL command for the Chishiki Agent TUI.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Globalization;
using System.Text;
using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;
using Chishiki.Agent.Core.Context;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Chishiki.Agent.Tui.Commands;

/// <summary>Settings for the interactive chat command.</summary>
public sealed class ChatCommandSettings : CommandSettings
{
    /// <summary>Gets or sets the model alias to use. Defaults to the configured default.</summary>
    [CommandOption("-m|--model")]
    public string? Model { get; set; }

    /// <summary>Gets or sets the system prompt to use for the session.</summary>
    [CommandOption("-s|--system")]
    public string? SystemPrompt { get; set; }

    /// <summary>Gets or sets a value indicating whether to disable streaming. Default is false (streaming enabled).</summary>
    [CommandOption("--no-stream")]
    public bool NoStream { get; set; }
}

/// <summary>Interactive chat REPL command providing a rich Spectre.Console terminal interface.</summary>
/// <remarks>Initializes a new instance of the <see cref="ChatCommand"/> class.</remarks>
/// <param name="orchestrator">The agent orchestrator.</param>
/// <param name="conversation">The conversation context for this session.</param>
public sealed class ChatCommand(IOrchestrator orchestrator, ConversationContext conversation) : AsyncCommand<ChatCommandSettings>
{
    private readonly IOrchestrator _orchestrator = orchestrator;

    private readonly ConversationContext _conversation = conversation;

    /// <inheritdoc/>
    protected override async Task<int> ExecuteAsync(CommandContext context, ChatCommandSettings settings, CancellationToken cancellationToken)
    {
        var model = settings.Model ?? "local";

        PrintBanner(model);

        if (!string.IsNullOrWhiteSpace(settings.SystemPrompt))
        {
            _conversation.AddMessage(new ChatMessage(ChatRole.System, settings.SystemPrompt));
        }

        AnsiConsole.MarkupLine("[grey]Type a message, or /help for commands. Ctrl+C to exit.[/]\n");

        while (true)
        {
            AnsiConsole.Markup("[bold green]You[/] [grey]>[/] ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input)) continue;

            // ── Slash commands ─────────────────────────────────────────────
            if (input.StartsWith('/'))
            {
                if (await HandleSlashCommandAsync(input, model)) continue;
                break;
            }

            // ── Chat request ───────────────────────────────────────────────
            _conversation.AddMessage(new ChatMessage(ChatRole.User, input));

            AnsiConsole.Markup("\n[bold blue]Agent[/] [grey]>[/] ");

            var request = new CompletionRequest(
                model,
                _conversation.GetTrimmedHistory(),
                Stream: !settings.NoStream);

            var responseText = new StringBuilder();

            try
            {
                if (!settings.NoStream)
                {
                    await foreach (var chunk in _orchestrator.StreamAsync(request, cancellationToken))
                    {
                        if (chunk.Delta is { } delta)
                        {
                            AnsiConsole.Write(delta);
                            _ = responseText.Append(delta);
                        }

                        if (chunk.IsFinished)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    await AnsiConsole.Status().StartAsync("Thinking…", async ctx =>
                    {
                        _ = ctx.Spinner(Spinner.Known.Dots);
                        var response = await _orchestrator.CompleteAsync(request, cancellationToken);
                        _ = responseText.Append(response.Message.Content);
                    });

                    AnsiConsole.Write(responseText.ToString());
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"\n[red]Error:[/] {Markup.Escape(ex.Message)}");
            }

            Console.WriteLine("\n");
            _conversation.AddMessage(new ChatMessage(ChatRole.Assistant, responseText.ToString()));
        }

        return 0;
    }

    private async Task<bool> HandleSlashCommandAsync(string input, string model)
    {
        var parts = input.Split(' ', 2);
        var cmd = parts[0].ToLowerInvariant();

        switch (cmd)
        {
            case "/help":
                PrintHelp();
                return true;

            case "/models":
                await PrintModelsAsync();
                return true;

            case "/providers":
                PrintProviders();
                return true;

            case "/plugins":
                PrintPlugins();
                return true;

            case "/clear":
                _conversation.Clear();
                AnsiConsole.Clear();
                PrintBanner(model);
                AnsiConsole.MarkupLine("[grey]Conversation cleared.[/]\n");
                return true;

            case "/exit":
            case "/quit":
            case "/q":
                return false;

            default:
                AnsiConsole.MarkupLine($"[yellow]Unknown command:[/] {Markup.Escape(cmd)}. Type /help for available commands.");
                return true;
        }
    }

    private static void PrintBanner(string model)
    {
        AnsiConsole.Write(new FigletText("Chishiki Agent").Color(Color.Blue));
        AnsiConsole.MarkupLine($"[dim]Model:[/] [cyan]{Markup.Escape(model)}[/]  [dim]|[/]  [dim]Type[/] [bold]/help[/] [dim]for commands[/]");
        AnsiConsole.WriteLine();
    }

    private static void PrintHelp()
    {
        var table = new Table().Border(TableBorder.Rounded);
        _ = table.AddColumn("[bold]Command[/]")
            .AddColumn("[bold]Description[/]")
            .AddRow("/models", "List available models")
            .AddRow("/providers", "Show provider status")
            .AddRow("/plugins", "List loaded plugins")
            .AddRow("/clear", "Clear conversation history")
            .AddRow("/exit | /quit | /q", "Exit the agent");
        AnsiConsole.Write(table);
    }

    private async Task PrintModelsAsync()
    {
        var models = await _orchestrator.ListModelsAsync();
        var table = new Table().Border(TableBorder.Rounded);
        _ = table.AddColumn("ID")
            .AddColumn("Provider")
            .AddColumn("Context");
        foreach (var m in models)
        {
            _ = table.AddRow(
                Markup.Escape(m.Id),
                Markup.Escape(m.Provider),
                m.ContextWindow.ToString("N0", CultureInfo.InvariantCulture));
        }

        AnsiConsole.Write(table);
    }

    private void PrintProviders()
    {
        var table = new Table().Border(TableBorder.Rounded);
        _ = table.AddColumn("Provider")
            .AddColumn("Status");
        foreach (var p in _orchestrator.Providers)
        {
            var status = p.IsAvailable ? "[green]available[/]" : "[red]unavailable[/]";
            _ = table.AddRow(Markup.Escape(p.DisplayName), status);
        }

        AnsiConsole.Write(table);
    }

    private void PrintPlugins()
    {
        var table = new Table().Border(TableBorder.Rounded);
        _ = table.AddColumn("Plugin")
            .AddColumn("Version")
            .AddColumn("Capabilities");
        foreach (var p in _orchestrator.Plugins)
        {
            _ = table.AddRow(
                Markup.Escape(p.DisplayName),
                Markup.Escape(p.Version),
                string.Join(", ", p.Capabilities.Select(c => Markup.Escape(c.Name))));
        }

        AnsiConsole.Write(table);
    }
}
