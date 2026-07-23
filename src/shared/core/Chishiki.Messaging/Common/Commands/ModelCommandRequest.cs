// -----------------------------------------------------------------------------
// File:        ModelCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base command request with model support.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Common.Commands;

/// <summary>Model command base class.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <seealso cref="CommandRequest" />
public abstract record ModelCommandRequest<TKey, TModel> : CommandRequest
    where TModel : class
{
    /// <summary>Initializes a new instance of the <see cref="ModelCommandRequest{TKey,TModel}"/> class. .</summary>
    /// <param name="model">The model.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    protected ModelCommandRequest(TModel model, Guid correlationId) : base(correlationId)
    {
        Model = model;
    }

    /// <summary>Gets the model. .</summary>
    /// <value>
    /// The model.
    /// </value>
    public TModel Model { get; init; }
}

