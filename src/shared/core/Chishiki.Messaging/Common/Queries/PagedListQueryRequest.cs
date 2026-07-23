// -----------------------------------------------------------------------------
// File:        PagedListQueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Paged list query request implementation for retrieving paginated results.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki;
using Chishiki.Messaging.Abstractions.Queries;

namespace Chishiki.Messaging.Common.Queries;

/// <summary>Paged list query request implementation.</summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public abstract record PagedListQueryRequest<TModel> : QueryRequest<IPagedList<TModel>>, IPagedListQueryRequest<TModel>
{
    /// <summary>Initializes a new instance of the <see cref="PagedListQueryRequest{TModel}"/> class. .</summary>
    /// <param name="correlationId">The correlation ID.</param>
    protected PagedListQueryRequest(Guid correlationId) : base(correlationId)
    {
    }
}
