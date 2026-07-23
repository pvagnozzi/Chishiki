// -----------------------------------------------------------------------------
// File:        CreateCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract create command request implementation.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions.Commands;
using Chishiki.Messaging.Common.Commands;

namespace Chishiki.Messaging.Common.Commands;

/// <summary>Create command base class.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <seealso cref="ModelCommandRequest{TKey,TModel}" />
public abstract record CreateCommandRequest<TKey, TModel> : ModelCommandRequest<TKey, TModel>,
    ICreateCommandRequest<TModel> where TModel : class
{
    /// <summary>Initializes a new instance of the <see cref="CreateCommandRequest{TKey,TModel}"/> class. .</summary>
    /// <param name="model">The model.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    protected CreateCommandRequest(TModel model, Guid correlationId) : base(model, correlationId)
    {
    }
}

