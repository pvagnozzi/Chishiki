// -----------------------------------------------------------------------------
// File:        ICreateCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for create command requests with model support.
// Created:     2024-07-20
// Modified:    2024-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Commands;

/// <summary>Create command request interface.</summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface ICreateCommandRequest<out TModel> : IModelCommandRequest<TModel> where TModel : class;

