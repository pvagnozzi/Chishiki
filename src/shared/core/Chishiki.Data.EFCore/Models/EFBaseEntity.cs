// -----------------------------------------------------------------------------
// File:        EFBaseEntity.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base entity class for all EF Core entities with Id, CreatedAt, and UpdatedAt properties.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Chishiki.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chishiki.Data.EFCore.Models;

/// <summary>Abstract base entity for all EF Core entities with generic key type, providing Id, CreatedAt, and UpdatedAt audit properties.</summary>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
// ReSharper disable once InconsistentNaming
public abstract class EFBaseEntity<TKey> : IEntity<TKey>, IEquatable<IEntity<TKey>>
{
    /// <summary>Gets the entity's primary key identifier. .</summary>
    [Key]
    [DisplayName("Id")]
    public virtual TKey Id { get; protected internal set; } = default!;

    /// <summary>Gets the date and time in UTC when this entity was created. .</summary>
    [Required]
    [DisplayName("Created On")]
    public DateTimeOffset CreatedAt { get; protected internal set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the date and time in UTC when this entity was last updated. .</summary>
    [Required]
    [DisplayName("Updated On")]
    public DateTimeOffset UpdatedAt { get; protected internal set; } = DateTimeOffset.UtcNow;

    /// <summary>Determines whether the specified object is equal to the current entity by comparing primary keys. .</summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>True if the specified object is an entity with the same primary key; otherwise false.</returns>
    public override bool Equals(object? obj)
    {
        var other = obj as IEntity<TKey>;
        return other is not null && ((IEquatable<IEntity<TKey>>)this).Equals(other);
    }

    /// <summary>Returns a hash code for this entity based on its primary key. .</summary>
    /// <returns>A hash code suitable for use in hashing algorithms and data structures.</returns>
    public override int GetHashCode() => Id?.GetHashCode() ?? -1;

    /// <summary>Indicates whether the current entity is equal to another entity of the same type by comparing primary keys. .</summary>
    /// <param name="other">The entity to compare with this entity.</param>
    /// <returns>True if both entities have the same primary key; otherwise false.</returns>
    bool IEquatable<IEntity<TKey>>.Equals(IEntity<TKey>? other) => other is not null && (Id?.Equals(other.Id) ?? false);
}

/// <summary>Extension methods for configuring EF Core entities that inherit from EFBaseEntity.</summary>
public static class EFBaseEntityExtensions
{
    /// <summary>Configures an entity type with audit tracking properties (Id, CreatedAt, UpdatedAt) for the model builder. .</summary>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <typeparam name="TEntity">The entity type to configure.</typeparam>
    /// <param name="modelBuilder">The model builder to configure.</param>
    /// <param name="tableName">Optional custom table name. If null, uses the entity type name.</param>
    /// <param name="customAction">Optional custom configuration action for the entity type builder.</param>
    /// <returns>The configured model builder for method chaining.</returns>
    public static ModelBuilder Configure<TKey, TEntity>(this ModelBuilder modelBuilder, string? tableName = null,
        Action<EntityTypeBuilder<TEntity>>? customAction = null)
        where TKey : IEquatable<TKey>
        where TEntity : class, IEntity<TKey>
    {
        tableName ??= typeof(TEntity).Name;
        _ = modelBuilder.Entity<TEntity>(tb =>
        {
            _ = tb
                .ToTable(tableName)
                .HasKey(e => e.Id);
            _ = tb
                .Property(e => e.Id);
            _ = tb
                .Property(e => e.CreatedAt)
                .IsRequired();
            _ = tb
                .Property(e => e.UpdatedAt)
                .IsRequired();
            customAction?.Invoke(tb);
        });

        return modelBuilder;
    }

    /// <summary>Fluent configuration method to set the entity's primary key identifier. .</summary>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entity">The entity to configure.</param>
    /// <param name="id">The value to assign to the entity's Id property.</param>
    /// <returns>The entity for method chaining.</returns>
    public static TEntity WithId<TKey, TEntity>(this TEntity entity, TKey id)
        where TKey : IEquatable<TKey>
        where TEntity : EFBaseEntity<TKey>
    {
        entity.Id = id;
        return entity;
    }

    /// <summary>Fluent configuration method to set the entity's creation timestamp. .</summary>
    /// <typeparam name="TEntity">The entity type (must use Guid as key).</typeparam>
    /// <param name="entity">The entity to configure.</param>
    /// <param name="createdOn">The value to assign to the entity's CreatedAt property.</param>
    /// <returns>The entity for method chaining.</returns>
    public static TEntity WithCreatedAt<TEntity>(this TEntity entity, DateTimeOffset createdOn)
        where TEntity : EFBaseEntity<Guid>
    {
        entity.CreatedAt = createdOn;
        return entity;
    }

    /// <summary>Fluent configuration method to set the entity's last update timestamp. .</summary>
    /// <typeparam name="TEntity">The entity type (must use Guid as key).</typeparam>
    /// <param name="entity">The entity to configure.</param>
    /// <param name="updatedOn">The value to assign to the entity's UpdatedAt property.</param>
    /// <returns>The entity for method chaining.</returns>
    public static TEntity WithUpdatedAt<TEntity>(this TEntity entity, DateTimeOffset updatedOn)
        where TEntity : EFBaseEntity<Guid>
    {
        entity.UpdatedAt = updatedOn;
        return entity;
    }
}
