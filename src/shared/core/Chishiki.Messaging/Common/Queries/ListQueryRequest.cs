// -----------------------------------------------------------------------------
// File:        ListQueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: List query request implementation for retrieving multiple items.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions.Queries;

namespace Chishiki.Messaging.Common.Queries;

/// <summary>List query request implementation.</summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public abstract record ListQueryRequest<TModel> : QueryRequest<IList<TModel>>, IListQueryRequest<TModel>
{
    /// <summary>Initializes a new instance of the <see cref="ListQueryRequest{TModel}"/> class. .</summary>
    /// <param name="correlationId">The correlation ID.</param>
    protected ListQueryRequest(Guid correlationId) : base(correlationId)
    {
    }
}
