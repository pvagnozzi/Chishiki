// -----------------------------------------------------------------------------
// File:        ConfigurationExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for IConfiguration and IServiceProvider providing typed configuration binding.
// Created:     2024-04-15
// Modified:    2026-05-03
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chishiki.Core;

/// <summary>Extension methods for <see cref="IConfiguration"/> and <see cref="IServiceProvider"/> providing typed configuration binding.</summary>
public static class ConfigurationExtensions
{
    /// <summary>Binds a configuration section to a strongly typed object.</summary>
    /// <typeparam name="T">The type to bind the configuration section to.</typeparam>
    /// <param name="configuration">The configuration root.</param>
    /// <param name="sectionName">The name of the configuration section to bind.</param>
    /// <returns>An instance of <typeparamref name="T"/> populated with values from the configuration section, or <c>null</c> if the section is not found.</returns>
    public static T? GetSection<T>(this IConfiguration configuration, string sectionName) where T : class, new() =>
        configuration.GetRequiredSection(sectionName).Get<T>();

    /// <summary>Retrieves a configured instance of the specified type from the dependency injection container.</summary>
    /// <typeparam name="T">The type of the configuration object to retrieve.</typeparam>
    /// <param name="serviceProvider">The service provider to resolve the configuration from.</param>
    /// <returns>An instance of <typeparamref name="T"/> with all configured options applied.</returns>
    public static T GetConfiguration<T>(this IServiceProvider serviceProvider) where T : class, new()
    {
        var configOptions = serviceProvider.GetRequiredService<IConfigureOptions<T>>();
        var config = new T();
        configOptions.Configure(config);
        return config;
    }
}
