// -----------------------------------------------------------------------------
// File:        IIdCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for ID-based command requests with type-safe key handling.
// Created:     2024-07-20
// Modified:    2024-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Commands;

/// <summary>ID command request interface.</summary>
/// <typeparam name="TKey">ID type.</typeparam>
public interface IIdCommandRequest<out TKey> : ICommandRequest
{
    /// <summary>Gets the ID value. .</summary>
    TKey Id { get; }
}

