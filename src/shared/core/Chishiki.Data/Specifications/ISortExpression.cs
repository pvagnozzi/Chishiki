// -----------------------------------------------------------------------------
// File:        ISortExpression.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for sort expressions in specifications.
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
public interface ISortExpression : IPropertyExpression
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="ISortExpression"/> is descending.
    /// </summary>
    /// <value>
    ///   <c>true</c> if descending; otherwise, <c>false</c>.
    /// </value>
    bool Descending { get; }
}


