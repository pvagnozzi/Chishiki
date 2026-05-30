// -----------------------------------------------------------------------------
// File:        IListQueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: List query interface for retrieving multiple items based on query criteria.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Queries;

/// <summary>List query interface.</summary>
/// <typeparam name="TResult">Result item type</typeparam>
public interface IListQueryRequest<TResult> : IQueryRequest<IList<TResult>>
{
}
