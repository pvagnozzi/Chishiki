// -----------------------------------------------------------------------------
// File:        QueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base record for query requests with generic model support.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions.Queries;
using Chishiki.Messaging.Common;

namespace Chishiki.Messaging.Common.Queries;

/// <summary>Query interface to be implemented by all query messages.</summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public abstract record QueryRequest<TModel> : RequestMessage, IQueryRequest<TModel>
{
    /// <summary>Initializes a new instance of the <see cref="QueryRequest{TModel}"/> class. .</summary>
    /// <param name="correlationId">The correlation identifier.</param>
    protected QueryRequest(Guid correlationId) : base(correlationId)
    {
    }
}

