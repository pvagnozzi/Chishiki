// -----------------------------------------------------------------------------
// File:        PluginBuilder.cs
// Author:      Piergiorgio Vagnozzi
// Description: Fluent builder for constructing IPlugin instances programmatically without subclassing PluginBase.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Abstractions.Interfaces;
using Chishiki.Agent.Abstractions.Models;

namespace Chishiki.Agent.Plugins.Sdk.Builder;

/// <summary>
/// Provides a fluent API for constructing <see cref="IPlugin"/> instances programmatically,
/// without requiring a dedicated subclass of <see cref="Base.PluginBase"/>.
/// </summary>
/// <remarks>
/// Example usage:
/// <code>
/// IPlugin plugin = PluginBuilder
///     .Create("my-plugin")
///     .WithDisplayName("My Plugin")
///     .WithVersion("1.0.0")
///     .WithDescription("Does something useful.")
///     .AddCapability("analyze", "Analyzes code", async (req, ct) =>
///     {
///         // ... implementation ...
///         return CapabilityResult.Ok("analysis complete");
///     })
///     .Build();
/// </code>
/// </remarks>
public sealed class PluginBuilder
{
    #region Private Fields

    private readonly string _id;
    private string _displayName;
    private string _version;
    private string _description;
    private readonly List<IPluginCapability> _capabilities;

    #endregion

    #region Constructor

    private PluginBuilder(string id)
    {
        _id = id;
        _displayName = id;
        _version = "1.0.0";
        _description = string.Empty;
        _capabilities = [];
    }

    #endregion

    #region Factory Method

    /// <summary>Creates a new <see cref="PluginBuilder"/> for the specified plugin identifier.</summary>
    /// <param name="id">The unique identifier for the plugin being built.</param>
    /// <returns>A new <see cref="PluginBuilder"/> instance.</returns>
    public static PluginBuilder Create(string id) => new(id);

    #endregion

    #region Fluent Configuration

    /// <summary>Sets the human-readable display name for the plugin.</summary>
    /// <param name="displayName">The display name to use.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PluginBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    /// <summary>Sets the semantic version string for the plugin.</summary>
    /// <param name="version">The version string, e.g. <c>1.0.0</c>.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PluginBuilder WithVersion(string version)
    {
        _version = version;
        return this;
    }

    /// <summary>Sets the description for the plugin.</summary>
    /// <param name="description">A brief description of what the plugin does.</param>
    /// <returns>This builder instance for chaining.</returns>
    public PluginBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>Adds a capability to the plugin using a delegate handler.</summary>
    /// <param name="name">The unique capability name within this plugin.</param>
    /// <param name="description">A brief description of what the capability does.</param>
    /// <param name="handler">
    /// The async delegate to invoke when the capability is executed.
    /// Receives a <see cref="CapabilityRequest"/> and a <see cref="CancellationToken"/>;
    /// must return <see cref="Task{TResult}"/> where <c>TResult</c> is <see cref="CapabilityResult"/>.
    /// </param>
    /// <returns>This builder instance for chaining.</returns>
    public PluginBuilder AddCapability(
        string name,
        string description,
        Func<CapabilityRequest, CancellationToken, Task<CapabilityResult>> handler)
    {
        _capabilities.Add(new DelegateCapability(name, description, handler));
        return this;
    }

    #endregion

    #region Build

    /// <summary>Constructs and returns the configured <see cref="IPlugin"/> instance.</summary>
    /// <returns>A new <see cref="IPlugin"/> containing the capabilities registered with this builder.</returns>
    public IPlugin Build() =>
        new BuiltPlugin(_id, _displayName, _version, _description, _capabilities.AsReadOnly());

    #endregion

    #region Nested Types

    /// <summary>An <see cref="IPluginCapability"/> backed by a delegate.</summary>
    private sealed class DelegateCapability : IPluginCapability
    {
        private static readonly Dictionary<string, string> s_emptySchema = [];
        private readonly Func<CapabilityRequest, CancellationToken, Task<CapabilityResult>> _handler;

        internal DelegateCapability(
            string name,
            string description,
            Func<CapabilityRequest, CancellationToken, Task<CapabilityResult>> handler)
        {
            Name = name;
            Description = description;
            _handler = handler;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Description { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> ParameterSchema => s_emptySchema;

        /// <inheritdoc/>
        public Task<CapabilityResult> ExecuteAsync(CapabilityRequest request, CancellationToken cancellationToken = default) =>
            _handler(request, cancellationToken);
    }

    /// <summary>An <see cref="IPlugin"/> constructed entirely from builder-supplied data.</summary>
    private sealed class BuiltPlugin : IPlugin
    {
        private bool _disposed;

        internal BuiltPlugin(
            string id,
            string displayName,
            string version,
            string description,
            IReadOnlyList<IPluginCapability> capabilities)
        {
            Id = id;
            DisplayName = displayName;
            Version = version;
            Description = description;
            Capabilities = capabilities;
        }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string DisplayName { get; }

        /// <inheritdoc/>
        public string Version { get; }

        /// <inheritdoc/>
        public string Description { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IPluginCapability> Capabilities { get; }

        /// <inheritdoc/>
        public Task InitializeAsync(IPluginContext context, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        /// <inheritdoc/>
        public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }

            return ValueTask.CompletedTask;
        }
    }

    #endregion
}
