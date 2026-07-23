// -----------------------------------------------------------------------------
// File:        EFStringEntity.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core entity base class for entities using string as primary key.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Chishiki.Data.EFCore.Models;

/// <summary>EF Core entity base class for entities using string as their primary key. Automatically generates a new Guid-based string if not explicitly set.</summary>
/// <seealso cref="EFBaseEntity{String}" />
public class EFStringEntity() : EFBaseEntity<string>
{
    /// <summary>Gets or sets the entity's string primary key identifier. Defaults to a Guid string representation if not explicitly set. .</summary>
    [Key]
    [Required]
    [MaxLength(64)]
    [DisplayName("Id")]
    public override string Id { get; protected internal set; } = Guid.NewGuid().ToString();
}
