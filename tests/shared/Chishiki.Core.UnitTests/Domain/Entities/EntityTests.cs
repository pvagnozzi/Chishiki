// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Core.Domain.Entities;
using Chishiki.Core.Domain.Events;

namespace Chishiki.Core.UnitTests.Domain.Entities;

[TestFixture, Category("Unit")]
public class EntityTests
{
    private sealed class StubEntity : Entity<Guid>
    {
        public StubEntity(Guid id) : base(id) { }
    }

    [Test]
    public void Equals_SameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var a = new StubEntity(id);
        var b = new StubEntity(id);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var a = new StubEntity(Guid.NewGuid());
        var b = new StubEntity(Guid.NewGuid());

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void EqualityOperator_SameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var a = new StubEntity(id);
        var b = new StubEntity(id);

        Assert.That(a == b, Is.True);
    }

    [Test]
    public void GetHashCode_SameId_ReturnsSameHash()
    {
        var id = Guid.NewGuid();
        var a = new StubEntity(id);
        var b = new StubEntity(id);

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }
}

[TestFixture, Category("Unit")]
public class AggregateRootTests
{
    private sealed class StubEvent : IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }

    private sealed class StubAggregate : AggregateRoot<int>
    {
        public StubAggregate(int id) : base(id) { }

        public void DoSomething() => RaiseDomainEvent(new StubEvent());
    }

    [Test]
    public void RaiseDomainEvent_AddsEventToDomainEvents()
    {
        var aggregate = new StubAggregate(1);

        aggregate.DoSomething();

        Assert.That(aggregate.DomainEvents, Has.Count.EqualTo(1));
    }

    [Test]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var aggregate = new StubAggregate(1);
        aggregate.DoSomething();
        aggregate.DoSomething();

        aggregate.ClearDomainEvents();

        Assert.That(aggregate.DomainEvents, Is.Empty);
    }
}
