// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Core.Domain.ValueObjects;

/// <summary>
/// Base class for value objects, providing structural equality based on all component values.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>Returns all values that participate in equality comparison for this value object.</summary>
    /// <returns>An enumerable of the component values that define equality.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is ValueObject other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(ValueObject? other) =>
        other is not null &&
        GetType() == other.GetType() &&
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    /// <inheritdoc/>
    public override int GetHashCode() =>
        GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate(HashCode.Combine);

    /// <summary>Determines whether two value objects are equal by component values.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><see langword="true"/> if both instances have identical component values.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        Equals(left, right);

    /// <summary>Determines whether two value objects differ in at least one component value.</summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns><see langword="true"/> if the value objects are not structurally equal.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !Equals(left, right);
}
