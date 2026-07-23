// -----------------------------------------------------------------------------
// File:        IQuery.cs
// Author:      Piergiorgio Vagnozzi
// Description: Query request interface to be implemented by all query messages.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Queries;

/// <summary>Query request interface to be implemented by all query messages.</summary>
/// <typeparam name="TResult">Result item type.</typeparam>
public interface IQueryRequest<out TResult> : IRequestMessage;

