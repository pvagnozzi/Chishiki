// -----------------------------------------------------------------------------
// File:        IDeleteByIdCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for delete by ID command requests with type-safe key handling.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Commands;

/// <summary>Delete by ID command request interface.</summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TEntity">Entity type.</typeparam>
public interface IDeleteByIdCommandRequest<out TKey, out TEntity> : IIdCommandRequest<TKey>
    where TEntity : class;


