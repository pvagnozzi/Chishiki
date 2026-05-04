// -----------------------------------------------------------------------------
// File:        EFGuidEntity.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core entity base class for entities using Guid as primary key.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Chishiki.Data.EFCore.Models;

/// <summary>
/// EF Core entity base class for entities using Guid as their primary key. Automatically generates a new Guid if not explicitly set.
/// </summary>
/// <seealso cref="EFBaseEntity{Guid}" />
// ReSharper disable once InconsistentNaming
public class EFGuidEntity : EFBaseEntity<Guid>
{
    /// <summary>
    /// Gets or sets the entity's Guid primary key identifier. Defaults to a new Guid if not explicitly set.
    /// </summary>
    [Key]
    [Required]
    [DisplayName("Id")]
    public override Guid Id { get; protected internal set; } = Guid.NewGuid();
}
