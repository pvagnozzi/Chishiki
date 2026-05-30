// -----------------------------------------------------------------------------
// File:        RequestMessageHandlerContext.cs
// Author:      Piergiorgio Vagnozzi
// Description: Request handler context implementation providing access to mapper, unit of work, and service provider.
// Created:     2026-05-17
// Modified:    2026-05-17
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Abstractions;
using Chishiki.Mapping;
using Chishiki.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chishiki.Messaging.Common;

/// <summary>Request handler context.</summary>
public record RequestMessageHandlerContext(IServiceProvider ServiceProvider) : IRequestMessageHandlerContext
{
    /// <summary>The mapper. .</summary>
    private IMapper? _mapper;

    /// <summary>The unit of work. .</summary>
    private IUnitOfWork? _unitOfWork;

    /// <summary>Gets the mapper. .</summary>
    public IMapper Mapper => _mapper ??= ServiceProvider.GetRequiredService<IMapper>();

    /// <summary>Gets the unit of work. .</summary>
    public IUnitOfWork UnitOfWork => _unitOfWork ??= ServiceProvider.GetRequiredService<IUnitOfWork>();
}

/// <summary>Request handler context extensions.</summary>
public static class RequestMessageHandlerContextExtensions
{
    /// <summary>Register the request message handler context. .</summary>
    /// <param name="services">Services to extend.</param>
    /// <returns>Services.</returns>
    public static IServiceCollection AddRequestMessageContext(this IServiceCollection services) =>
        services.Any(x => x.ServiceType == typeof(IRequestMessageHandlerContext))
            ? services
            : services.AddScoped<IRequestMessageHandlerContext>(ctx => new RequestMessageHandlerContext(ctx));
}
