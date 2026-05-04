// -----------------------------------------------------------------------------
// File:        SortExpression.cs
// Author:      Piergiorgio Vagnozzi
// Description: Record representing a sort expression in specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Specifications;

/// <summary>
/// Sort Expression.
/// </summary>
/// <seealso cref="PropertyExpression" />
public record SortExpression(string PropertyName, bool Descending = false) : PropertyExpression(PropertyName), ISortExpression;


