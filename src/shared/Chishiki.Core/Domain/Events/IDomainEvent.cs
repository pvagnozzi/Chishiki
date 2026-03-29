// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Core.Domain.Events;

/// <summary>Marker interface for domain events raised by aggregate roots.</summary>
public interface IDomainEvent
{
    /// <summary>Gets the UTC instant at which the event occurred.</summary>
    DateTime OccurredAt { get; }
}
