// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Core.Domain.ValueObjects;

namespace Chishiki.Core.UnitTests.Domain.ValueObjects;

[TestFixture, Category("Unit")]
public class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Test]
    public void Equals_SameComponents_ReturnsTrue()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_DifferentAmount_ReturnsFalse()
    {
        var a = new Money(10m, "USD");
        var b = new Money(20m, "USD");

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void EqualityOperator_SameComponents_ReturnsTrue()
    {
        var a = new Money(5m, "EUR");
        var b = new Money(5m, "EUR");

        Assert.That(a == b, Is.True);
    }

    [Test]
    public void GetHashCode_SameComponents_ReturnsSameHash()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }
}
