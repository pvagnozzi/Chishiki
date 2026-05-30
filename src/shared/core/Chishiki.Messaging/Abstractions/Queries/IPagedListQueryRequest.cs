// -----------------------------------------------------------------------------
// File:        IPagedListQueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Paged list query interface for retrieving paginated results.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki;

namespace Chishiki.Messaging.Abstractions.Queries;

/// <summary>Paged list query interface.</summary>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IPagedListQueryRequest<TResult> : IQueryRequest<IPagedList<TResult>>
{
}
