// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Core.Domain.Events;

namespace Chishiki.Core.Domain.Entities;

/// <summary>
/// Base class for aggregate roots, extending <see cref="Entity{TId}"/> with domain-event collection.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Initializes a new instance of <see cref="AggregateRoot{TId}"/> with the given identifier.</summary>
    /// <param name="id">The unique identifier for this aggregate root.</param>
    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>Gets the uncommitted domain events raised by this aggregate since the last clear.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Enqueues <paramref name="domainEvent"/> in the pending events collection.</summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>Removes all pending domain events, typically called after they have been dispatched.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
