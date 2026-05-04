// Modified:    2026-05-04

namespace Chishiki.Data.Audit;

/// <summary>
/// Record representing a property-level change in an audit trail entry.
/// </summary>
/// <param name="PropertyName">The name of the property that changed.</param>
/// <param name="OldValue">The old value of the property before the change.</param>
/// <param name="NewValue">The new value of the property after the change.</param>
public record PropertyChange(string PropertyName, string? OldValue = null, string? NewValue = null);
