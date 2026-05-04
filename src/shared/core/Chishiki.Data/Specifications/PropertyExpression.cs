// -----------------------------------------------------------------------------
// File:        PropertyExpression.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract record representing a property expression in specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Specifications;

/// <summary>
/// Property Expression.
/// </summary>
/// <seealso cref="IPropertyExpression" />
public abstract record PropertyExpression(string PropertyName) : IPropertyExpression;


