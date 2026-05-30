// -----------------------------------------------------------------------------
// File:        IPropertyExpression.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for property expressions in specifications.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Specifications;

/// <summary>Property Expression.</summary>
public interface IPropertyExpression
{
    /// <summary>Gets the name of the property. .</summary>
    /// <value>
    /// The name of the property.
    /// </value>
    string PropertyName { get; }
}


