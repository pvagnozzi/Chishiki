// -----------------------------------------------------------------------------
// File:        ModelBuilderExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core extensions for configuring base entities with EntityTypeBuilder.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.EFCore.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chishiki.Data.EFCore;

/// <summary>Extensions for EntityTypeBuilder to configure base entities with common properties like Id, CreatedOn, and UpdatedOn.</summary>
public static class ModelBuilderExtensions
{
    /// <summary>Configures an entity type as a base entity with a Guid primary key. Sets up primary key constraints and required properties. .</summary>
    /// <typeparam name="TEntity">The entity type that extends EFBaseEntity with Guid key.</typeparam>
    /// <param name="builder">The entity type builder to configure.</param>
    /// <returns>The configured entity type builder for method chaining.</returns>
    public static EntityTypeBuilder<TEntity> SetGuidBaseEntity<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : EFBaseEntity<Guid> => SetBaseEntity<Guid, TEntity>(builder);

    /// <summary>Configures an entity type as a base entity with the specified key type. Sets up primary key constraints and required properties (Id, CreatedOn, UpdatedOn). .</summary>
    /// <typeparam name="TKey">The type of the primary key.</typeparam>
    /// <typeparam name="TEntity">The entity type that extends EFBaseEntity.</typeparam>
    /// <param name="builder">The entity type builder to configure.</param>
    /// <returns>The configured entity type builder for method chaining.</returns>
    public static EntityTypeBuilder<TEntity> SetBaseEntity<TKey, TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : EFBaseEntity<TKey>
    {
        _ = builder
            .HasKey(x => x.Id);
        _ = builder
            .Property(x => x.Id)
            .HasMaxLength(64)
            .IsRequired();
        _ = builder
            .Property(x => x.CreatedOn)
            .IsRequired();
        _ = builder
            .Property(x => x.UpdatedOn)
            .IsRequired();
        return builder;
    }
}
