// -----------------------------------------------------------------------------
// File:        CommandRequest.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base record for command requests.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions.Commands;
using Chishiki.Messaging.Common;

namespace Chishiki.Messaging.Common.Commands;

/// <summary>Command base class.</summary>
/// <seealso cref="RequestMessage" />
public abstract record CommandRequest : RequestMessage, ICommandRequest
{
    /// <summary>Initializes a new instance of the <see cref="CommandRequest"/> class. .</summary>
    /// <param name="correlationId">The correlation identifier.</param>
    protected CommandRequest(Guid correlationId) : base(correlationId)
    {
    }
}

