// -----------------------------------------------------------------------------
// File:        IIncludeExpression.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for include expressions in specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Specifications;

/// <summary>
/// Include Expression.
/// </summary>
public interface IIncludeExpression : IPropertyExpression
{
    /// <summary>
    /// Gets the nested expressions.
    /// </summary>
    /// <value>
    /// The nested expressions.
    /// </value>
    IIncludeExpression[] NestedExpressions { get; }
}


