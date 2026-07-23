// -----------------------------------------------------------------------------
// File:        IUpdateByIdCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for update by ID command requests combining ID and model operations.
// Created:     2024-07-20
// Modified:    2024-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Commands;

/// <summary>Update by ID command request interface.</summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface IUpdateByIdCommandRequest<out TKey, out TModel> : IIdCommandRequest<TKey>, IModelCommandRequest<TModel> where TModel : class;

