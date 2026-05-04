// -----------------------------------------------------------------------------
// File:        EntityAttribute.cs
// Author:      Piergiorgio Vagnozzi
// Description: Attribute for marking entity classes as soft-deletable or auditable.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Models;

/// <summary>
/// Entity attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class EntityAttribute(bool isSoftDeletable = false, bool isAuditable = false) : Attribute
{

    /// <summary>
    /// Gets or sets a value indicating whether this instance can be delete in the soft-way.
    /// </summary>
    public bool IsSoftDeletable { get; init; } = isSoftDeletable;


    /// <summary>
    /// Gets or sets a value indicating whether this instance is auditable.
    /// </summary>
    public bool IsAuditable { get; init; } = isAuditable;
}

