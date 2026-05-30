// -----------------------------------------------------------------------------
// File:        UpdateByIdCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract update by ID command request implementation.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions.Commands;

namespace Chishiki.Messaging.Common.Commands;

/// <summary>Update by id command.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <seealso cref="ModelCommandRequest{TKey,TModel}" />
/// <seealso cref="IUpdateByIdCommandRequest{TKey,TModel}" />
public abstract record UpdateByIdCommandRequest<TKey, TModel> : ModelCommandRequest<TKey, TModel>, IUpdateByIdCommandRequest<TKey, TModel>
    where TModel : class
{
    /// <summary>Initializes a new instance of the <see cref="UpdateByIdCommandRequest{TKey,TModel}"/> class. .</summary>
    /// <param name="id">The identifier.</param>
    /// <param name="model">The model.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    protected UpdateByIdCommandRequest(TKey id, TModel model, Guid correlationId) : base(model, correlationId)
    {
        Id = id;
    }

    /// <summary>Gets the identifier. .</summary>
    /// <value>
    /// The identifier.
    /// </value>
    public TKey Id { get; init; }
}

