// -----------------------------------------------------------------------------
// File:        IModelCommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for command requests that carry a model payload.
// Created:     2024-07-20
// Modified:    2024-07-20
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Commands;

/// <summary>Model command request interface.</summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface IModelCommandRequest<out TModel> : ICommandRequest
    where TModel : class
{
    /// <summary>Gets the model payload. .</summary>
    TModel Model { get; }
}

