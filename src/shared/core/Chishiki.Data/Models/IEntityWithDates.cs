// -----------------------------------------------------------------------------
// File:        IEntityWithDates.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for entities with automatic date tracking (CreatedOn, UpdatedOn).
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Models;

/// <summary>
/// Interface for entities with automatic date tracking (CreatedOn, UpdatedOn).
/// </summary>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IEntityWithDates<out TKey> : IEntity<TKey>
{
    /// <summary>
    /// Gets the created on.
    /// </summary>
    /// <value>
    /// The created on.
    /// </value>
    DateTimeOffset CreatedOn { get; }

    /// <summary>
    /// Gets the updated on.
    /// </summary>
    /// <value>
    /// The updated on.
    /// </value>
    DateTimeOffset UpdatedOn { get; }
}

