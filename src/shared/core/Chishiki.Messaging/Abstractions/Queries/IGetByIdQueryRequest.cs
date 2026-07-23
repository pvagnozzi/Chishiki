// -----------------------------------------------------------------------------
// File:        IGetByIdQueryRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Query request interface for retrieving an entity by its ID.
// Created:     2024-07-20
// Modified:    2024-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Queries;

/// <summary>Get by ID query request interface.</summary>
/// <typeparam name="TKey">ID type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IGetByIdQueryRequest<out TKey, out TResult> : IQueryRequest<TResult?>
{
    /// <summary>Gets the ID value. .</summary>
    TKey Id { get; }
}

