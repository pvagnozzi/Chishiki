// -----------------------------------------------------------------------------
// File:        ICommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base interface for all command requests in the messaging system.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions.Commands;

/// <summary>Command request interface to be implemented by all command messages.</summary>
public interface ICommandRequest : IRequestMessage;

