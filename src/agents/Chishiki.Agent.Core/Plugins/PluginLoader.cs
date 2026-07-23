// -----------------------------------------------------------------------------
// File:        PluginLoader.cs
// Author:      Piergiorgio Vagnozzi
// Description: Loads and unloads plugin assemblies using isolated AssemblyLoadContexts.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Plugins.Sdk.Attributes;
using Chishiki.Agent.Plugins.Sdk.Manifest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Chishiki.Agent.Core.Plugins;

/// <summary>Loads, tracks, and unloads plugin assemblies in isolated <see cref="AssemblyLoadContext"/> instances.</summary>
/// <remarks>Initializes a new instance of the <see cref="PluginLoader"/> class.</remarks>
/// <param name="services">Host service provider passed to each plugin's context.</param>
/// <param name="configuration">Host configuration passed to each plugin's context.</param>
/// <param name="loggerFactory">Logger factory for creating plugin-scoped loggers.</param>
/// <param name="logger">Logger for the loader itself.</param>
public sealed partial class PluginLoader(
    IServiceProvider services,
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    ILogger<PluginLoader> logger)
{
    #region Fields

    private readonly ConcurrentDictionary<string, LoadedPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _services = services;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly ILogger<PluginLoader> _logger = logger;

    #endregion

    #region Public API

    /// <summary>Gets the currently loaded plugins.</summary>
    public IReadOnlyDictionary<string, IPlugin> LoadedPlugins =>
        _plugins.ToDictionary(kv => kv.Key, kv => kv.Value.Plugin);

    /// <summary>Loads a single plugin assembly from the specified path.</summary>
    /// <param name="assemblyPath">Absolute path to the plugin assembly (.dll).</param>
    /// <param name="pluginDirectory">Directory used as the plugin's working directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded <see cref="IPlugin"/> instance.</returns>
    public async Task<IPlugin> LoadAsync(
        string assemblyPath,
        string? pluginDirectory = null,
        CancellationToken cancellationToken = default)
    {
        LogPluginLoading(_logger, assemblyPath);
        pluginDirectory ??= Path.GetDirectoryName(assemblyPath) ?? ".";

        var context = new PluginAssemblyLoadContext(assemblyPath);
        Assembly assembly;
        try
        {
            assembly = context.LoadFromAssemblyPath(assemblyPath);
        }
        catch (Exception ex)
        {
            LogPluginLoadFailed(_logger, assemblyPath, ex);
            context.Unload();
            throw;
        }

        // Find type decorated with [Plugin]
        var pluginType = assembly.GetExportedTypes()
            .FirstOrDefault(t => t.GetCustomAttribute<PluginAttribute>() is not null
                              && t.IsAssignableTo(typeof(IPlugin))
                              && !t.IsAbstract);

        if (pluginType is null)
        {
            context.Unload();
            throw new InvalidOperationException(
                $"No public non-abstract class implementing IPlugin with [Plugin] attribute found in '{assemblyPath}'.");
        }

        IPlugin plugin;
        try
        {
            plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
        }
        catch (Exception ex)
        {
            LogPluginLoadFailed(_logger, assemblyPath, ex);
            context.Unload();
            throw;
        }

        var pluginLogger = _loggerFactory.CreateLogger(pluginType.FullName ?? pluginType.Name);
        var ctx = new PluginContext(_services, _configuration, pluginLogger, pluginDirectory);

        try
        {
            await plugin.InitializeAsync(ctx, cancellationToken);
        }
        catch (Exception ex)
        {
            LogPluginInitFailed(_logger, plugin.Id, ex);
            await plugin.DisposeAsync();
            context.Unload();
            throw;
        }

        _plugins[plugin.Id] = new LoadedPlugin(plugin, context);
        LogPluginLoaded(_logger, plugin.Id, plugin.DisplayName, plugin.Version);
        return plugin;
    }

    /// <summary>Loads all plugins discovered in a directory by scanning for <c>plugin.json</c> manifests.</summary>
    /// <param name="directory">Root directory to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task LoadFromDirectoryAsync(string directory, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directory))
        {
            LogPluginDirectoryNotFound(_logger, directory);
            return;
        }

        var manifestFiles = Directory.GetFiles(directory, "plugin.json", SearchOption.AllDirectories);
        LogScanningDirectory(_logger, directory, manifestFiles.Length);

        foreach (var manifestPath in manifestFiles)
        {
            if (cancellationToken.IsCancellationRequested) break;
            try
            {
                var json = await File.ReadAllTextAsync(manifestPath, cancellationToken);
                var manifest = JsonSerializer.Deserialize<PluginManifest>(json);
                if (manifest is null) continue;

                var pluginDir = Path.GetDirectoryName(manifestPath)!;
                var assemblyPath = Path.Combine(pluginDir, manifest.EntryAssembly);
                if (!File.Exists(assemblyPath))
                {
                    LogPluginAssemblyMissing(_logger, manifest.Id, assemblyPath);
                    continue;
                }

                _ = await LoadAsync(assemblyPath, pluginDir, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogPluginLoadFailed(_logger, manifestPath, ex);
            }
        }
    }

    /// <summary>Shuts down and unloads the plugin with the specified ID.</summary>
    /// <param name="pluginId">The plugin ID to unload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task UnloadAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        if (!_plugins.TryRemove(pluginId, out var loaded))
        {
            LogPluginNotFound(_logger, pluginId);
            return;
        }

        LogPluginUnloading(_logger, pluginId);
        await loaded.Plugin.ShutdownAsync(cancellationToken);
        await loaded.Plugin.DisposeAsync();
        loaded.Context.Unload();
        LogPluginUnloaded(_logger, pluginId);
    }

    #endregion

    #region Nested Types

    private sealed record LoadedPlugin(IPlugin Plugin, AssemblyLoadContext Context);

    private sealed class PluginAssemblyLoadContext(string pluginPath) : AssemblyLoadContext(isCollectible: true)
    {
        private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var path = _resolver.ResolveAssemblyToPath(assemblyName);
            return path is not null ? LoadFromAssemblyPath(path) : null;
        }
    }

    #endregion

    #region Logging

    [LoggerMessage(EventId = 2010, Level = LogLevel.Debug,
        Message = "Loading plugin from '{Path}'.")]
    private static partial void LogPluginLoading(ILogger logger, string path);

    [LoggerMessage(EventId = 2011, Level = LogLevel.Information,
        Message = "Plugin '{PluginId}' ('{DisplayName}' v{Version}) loaded successfully.")]
    private static partial void LogPluginLoaded(ILogger logger, string pluginId, string displayName, string version);

    [LoggerMessage(EventId = 2012, Level = LogLevel.Error,
        Message = "Failed to load plugin assembly from '{Path}'.")]
    private static partial void LogPluginLoadFailed(ILogger logger, string path, Exception exception);

    [LoggerMessage(EventId = 2013, Level = LogLevel.Error,
        Message = "Plugin '{PluginId}' initialisation failed.")]
    private static partial void LogPluginInitFailed(ILogger logger, string pluginId, Exception exception);

    [LoggerMessage(EventId = 2014, Level = LogLevel.Debug,
        Message = "Unloading plugin '{PluginId}'.")]
    private static partial void LogPluginUnloading(ILogger logger, string pluginId);

    [LoggerMessage(EventId = 2015, Level = LogLevel.Information,
        Message = "Plugin '{PluginId}' unloaded.")]
    private static partial void LogPluginUnloaded(ILogger logger, string pluginId);

    [LoggerMessage(EventId = 2016, Level = LogLevel.Warning,
        Message = "Plugin directory '{Directory}' not found; skipping plugin discovery.")]
    private static partial void LogPluginDirectoryNotFound(ILogger logger, string directory);

    [LoggerMessage(EventId = 2017, Level = LogLevel.Debug,
        Message = "Scanning '{Directory}' for plugins; found {Count} manifest(s).")]
    private static partial void LogScanningDirectory(ILogger logger, string directory, int count);

    [LoggerMessage(EventId = 2018, Level = LogLevel.Warning,
        Message = "Assembly for plugin '{PluginId}' not found at '{Path}'; skipping.")]
    private static partial void LogPluginAssemblyMissing(ILogger logger, string pluginId, string path);

    [LoggerMessage(EventId = 2019, Level = LogLevel.Warning,
        Message = "Plugin '{PluginId}' not found in loaded plugins.")]
    private static partial void LogPluginNotFound(ILogger logger, string pluginId);

    #endregion
}
