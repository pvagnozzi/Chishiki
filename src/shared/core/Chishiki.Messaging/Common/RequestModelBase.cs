// -----------------------------------------------------------------------------
// File:        RequestModelBase.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base record for request models with generic key support.
// Created:     2024-09-19
// Modified:    2024-09-19
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Common;

/// <summary>Request model base class.</summary>
public record RequestModelBase<TKey>
{
    /// <summary>Gets the identifier. .</summary>
    /// <value>
    /// The identifier.
    /// </value>
    public TKey Id { get; init; } = default!;

    /// <summary>Gets the created on. .</summary>
    /// <value>
    /// The created on.
    /// </value>
    public DateTimeOffset CreatedOn { get; init; }

    /// <summary>Gets the updated on. .</summary>
    /// <value>
    /// The updated on.
    /// </value>
    public DateTimeOffset UpdatedOn { get; init; }
}
