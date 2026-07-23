// -----------------------------------------------------------------------------
// File:        PluginBase.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base class for Chishiki plugins, providing lifecycle management and capability discovery via reflection.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Reflection;
using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;
using Chishiki.Agent.Plugins.Sdk.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Chishiki.Agent.Plugins.Sdk.Base;

/// <summary>
/// Abstract base class for Chishiki agent plugins.
/// Manages the plugin lifecycle (initialize / shutdown / dispose), exposes the
/// <see cref="Logger"/> and <see cref="Context"/> to subclasses, and automatically
/// discovers public instance methods decorated with <see cref="CapabilityAttribute"/>
/// to register them as <see cref="IPluginCapability"/> entries.
/// </summary>
/// <remarks>
/// Subclasses should decorate the class with <see cref="PluginAttribute"/> to supply metadata,
/// and override <see cref="OnInitializeAsync"/> and <see cref="OnShutdownAsync"/> to handle
/// custom lifecycle work.
/// </remarks>
public abstract partial class PluginBase : IPlugin
{
    #region Private Fields

    private readonly List<IPluginCapability> _capabilities = [];
    private IPluginContext? _context;
    private bool _initialized;
    private bool _disposed;

    #endregion

    #region Properties

    /// <summary>Gets the unique identifier for this plugin. Reads from <see cref="PluginAttribute"/> when present; falls back to the type name.</summary>
    public virtual string Id => GetPluginAttribute()?.Id ?? GetType().Name;

    /// <summary>Gets the human-readable display name for this plugin. Reads from <see cref="PluginAttribute"/> when present; falls back to the type name.</summary>
    public virtual string DisplayName => GetPluginAttribute()?.DisplayName ?? GetType().Name;

    /// <summary>Gets the semantic version string for this plugin. Reads from <see cref="PluginAttribute"/> when present; defaults to <c>1.0.0</c>.</summary>
    public virtual string Version => GetPluginAttribute()?.Version ?? "1.0.0";

    /// <summary>Gets a brief description of this plugin. Reads from <see cref="PluginAttribute"/> when present; defaults to an empty string.</summary>
    public virtual string Description => GetPluginAttribute()?.Description ?? string.Empty;

    /// <summary>Gets the list of capabilities discovered and registered for this plugin.</summary>
    public IReadOnlyList<IPluginCapability> Capabilities => _capabilities;

    /// <summary>
    /// Gets the logger provided by the host context. Returns <see cref="NullLogger.Instance"/> before the
    /// plugin has been initialized.
    /// </summary>
    protected ILogger Logger => _context?.Logger ?? NullLogger.Instance;

    /// <summary>Gets the plugin context after initialization.</summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed before <see cref="InitializeAsync"/> has completed.</exception>
    protected IPluginContext Context => _context
        ?? throw new InvalidOperationException($"Plugin '{Id}' has not been initialized. Access Context only after InitializeAsync has completed.");

    #endregion

    #region Async Methods

    /// <summary>
    /// Initializes the plugin: stores the context, discovers reflection-based capabilities,
    /// and invokes <see cref="OnInitializeAsync"/> for subclass-specific setup.
    /// </summary>
    /// <param name="context">The plugin context providing services, configuration, and logging.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the plugin has already been disposed.</exception>
    public async Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _context = context;
        DiscoverCapabilities();

        LogPluginInitializing(Logger, Id);

        try
        {
            await OnInitializeAsync(cancellationToken).ConfigureAwait(false);
            _initialized = true;
            LogPluginInitialized(Logger, Id);
        }
        catch (Exception ex)
        {
            LogInitializationFailed(Logger, Id, ex);
            throw;
        }
    }

    /// <summary>
    /// Shuts the plugin down gracefully, invoking <see cref="OnShutdownAsync"/> for subclass-specific cleanup.
    /// No-op when the plugin has not been initialized.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            return;
        }

        LogPluginShuttingDown(Logger, Id);
        await OnShutdownAsync(cancellationToken).ConfigureAwait(false);
        _initialized = false;
        LogPluginShutDown(Logger, Id);
    }

    /// <summary>
    /// Disposes the plugin by invoking <see cref="ShutdownAsync"/> if still initialized, then
    /// calling <see cref="OnDispose"/> for subclass-specific resource release.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_initialized)
        {
            await ShutdownAsync().ConfigureAwait(false);
        }

        OnDispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Protected Virtual Methods

    /// <summary>
    /// Override to perform subclass-specific initialization work after the context has been stored
    /// and capabilities have been discovered.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected virtual Task OnInitializeAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Override to perform subclass-specific shutdown work before the plugin is marked as uninitialized.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected virtual Task OnShutdownAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Override to release unmanaged or managed resources during disposal. Called after <see cref="OnShutdownAsync"/>.</summary>
    protected virtual void OnDispose() { }

    #endregion

    #region Private Helpers

    private PluginAttribute? GetPluginAttribute() =>
        GetType().GetCustomAttribute<PluginAttribute>();

    private void DiscoverCapabilities()
    {
        _capabilities.Clear();

        var methods = GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<CapabilityAttribute>() is not null);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<CapabilityAttribute>()!;
            var capability = new ReflectedCapability(attr.Name, attr.Description, method, this);
            _capabilities.Add(capability);
            LogCapabilityDiscovered(Logger, attr.Name, Id);
        }
    }

    #endregion

    #region Log Messages

    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Plugin {PluginId} is initializing.")]
    private static partial void LogPluginInitializing(ILogger logger, string PluginId);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Plugin {PluginId} initialized successfully.")]
    private static partial void LogPluginInitialized(ILogger logger, string PluginId);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Plugin {PluginId} is shutting down.")]
    private static partial void LogPluginShuttingDown(ILogger logger, string PluginId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Plugin {PluginId} shut down successfully.")]
    private static partial void LogPluginShutDown(ILogger logger, string PluginId);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Debug, Message = "Discovered capability {CapabilityName} on plugin {PluginId}.")]
    private static partial void LogCapabilityDiscovered(ILogger logger, string CapabilityName, string PluginId);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Error, Message = "Plugin {PluginId} failed during initialization.")]
    private static partial void LogInitializationFailed(ILogger logger, string PluginId, Exception exception);

    #endregion

    #region Nested Types

    /// <summary>Wraps a method discovered via reflection as an <see cref="IPluginCapability"/>.</summary>
    private sealed class ReflectedCapability : IPluginCapability
    {
        private readonly MethodInfo _method;
        private readonly PluginBase _plugin;

        internal ReflectedCapability(string name, string description, MethodInfo method, PluginBase plugin)
        {
            Name = name;
            Description = description;
            _method = method;
            _plugin = plugin;
            ParameterSchema = BuildSchema(method);
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Description { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> ParameterSchema { get; }

        /// <inheritdoc/>
        public Task<CapabilityResult> ExecuteAsync(CapabilityRequest request, CancellationToken cancellationToken = default)
        {
            var result = _method.Invoke(_plugin, [request, cancellationToken]);

            return result is Task<CapabilityResult> task
                ? task
                : Task.FromResult(CapabilityResult.Fail(
                $"Capability method '{Name}' returned an unexpected type. Expected Task<CapabilityResult>."));
        }

        private static Dictionary<string, string> BuildSchema(MethodInfo method)
        {
            var schema = new Dictionary<string, string>();

            foreach (var param in method.GetParameters())
            {
                var attr = param.GetCustomAttribute<ParameterAttribute>();
                if (attr is not null)
                {
                    schema[attr.Name] = attr.Description;
                }
            }

            return schema;
        }
    }

    #endregion
}
