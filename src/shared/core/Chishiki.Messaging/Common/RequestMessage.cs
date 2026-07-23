// -----------------------------------------------------------------------------
// File:        RequestMessage.cs
// Author:      Piergiorgio Vagnozzi
// Description: Request message base class for all request types.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions;

namespace Chishiki.Messaging.Common;

/// <summary>Request message base class.</summary>
/// <param name="CorrelationId">Correlation ID.</param>
public abstract record RequestMessage(Guid CorrelationId) : IRequestMessage;
