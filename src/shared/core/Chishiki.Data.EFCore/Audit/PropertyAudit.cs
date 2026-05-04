// -----------------------------------------------------------------------------
// File:        PropertyAudit.cs
// Author:      Piergiorgio Vagnozzi
// Description: Immutable record representing a property audit entry with property name and old/new values.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Audit;

namespace Chishiki.Data.EFCore.Audit;

/// <summary>
/// Immutable audit record representing a change to a single entity property with old and new values.
/// </summary>
/// <param name="PropertyName">The name of the property that was changed.</param>
/// <param name="OldValue">The value of the property before the change, or null if not available.</param>
/// <param name="NewValue">The value of the property after the change, or null if not available.</param>
public record PropertyAudit(string PropertyName, string? OldValue, string? NewValue) : IPropertyAudit;
