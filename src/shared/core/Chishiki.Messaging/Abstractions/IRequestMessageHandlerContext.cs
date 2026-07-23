// -----------------------------------------------------------------------------
// File:        IRequestMessageHandlerContext.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for request message handler context providing access to mapper, unit of work, and service provider.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Abstractions;
using Chishiki.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace Chishiki.Messaging.Abstractions;

/// <summary>Represents a request message handler context.</summary>
public interface IRequestMessageHandlerContext
{
    /// <summary>Gets the mapper. .</summary>
    IMapper Mapper { get; }

    /// <summary>Gets the unit of work. .</summary>
    IUnitOfWork UnitOfWork { get; }

    /// <summary>Gets the service provider. .</summary>
    IServiceProvider ServiceProvider { get; }
}

/// <summary>Request handler context extensions.</summary>
public static class IRequestMessageHandlerContextExtensions
{
    /// <summary>Gets a required service from the service provider. .</summary>
    /// <param name="context">Service context.</param>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Service instance.</returns>
    public static TService GetRequiredService<TService>(this IRequestMessageHandlerContext context) where TService : notnull =>
        context.ServiceProvider.GetRequiredService<TService>();

    /// <summary>Gets a service from the service provider. .</summary>
    /// <param name="context">Service context.</param>
    /// <typeparam name="TService">Service type.</typeparam>
    /// <returns>Service instance or null if not found.</returns>
    public static TService? GetService<TService>(this IRequestMessageHandlerContext context) =>
        context.ServiceProvider.GetService<TService>();
}
