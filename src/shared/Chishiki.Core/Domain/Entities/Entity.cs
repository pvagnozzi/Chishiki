// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Core.Domain.Entities;

/// <summary>
/// Base class for all domain entities, providing identity-based equality semantics.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>Gets the unique identifier of this entity.</summary>
    public TId Id { get; protected set; }

    /// <summary>Initializes a new instance of <see cref="Entity{TId}"/> with the given identifier.</summary>
    /// <param name="id">The unique identifier for this entity.</param>
    protected Entity(TId id) => Id = id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is Entity<TId> entity && Equals(entity);

    /// <inheritdoc/>
    public bool Equals(Entity<TId>? other) =>
        other is not null && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        EqualityComparer<TId>.Default.GetHashCode(Id);

    /// <summary>Determines whether two entity instances are equal by identity.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><see langword="true"/> if both instances share the same identity.</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        Equals(left, right);

    /// <summary>Determines whether two entity instances are not equal by identity.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><see langword="true"/> if the two instances have different identities.</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !Equals(left, right);
}
