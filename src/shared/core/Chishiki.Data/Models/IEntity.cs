// -----------------------------------------------------------------------------
// File:        IEntity.cs
// Author:      Piergiorgio Vagnozzi
// Description: Generic interface for entities with a strongly-typed primary key.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Models;

/// <summary>Generic interface for entities with identity.</summary>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IEntity<out TKey>
{
    /// <summary>Gets the unique identifier for this entity.</summary>
    /// <value>The entity's primary key value.</value>
    TKey Id { get; }

    /// <summary>
    /// Gets the date and time when the entity was created. This property is typically used for auditing purposes to track when the entity was first persisted.
    /// </summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the date and time when the entity was last updated. This property is typically used for concurrency control and auditing purposes.
    /// </summary>
    DateTimeOffset UpdatedAt { get; }
}
