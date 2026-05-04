// Modified:    2026-05-04

namespace Chishiki.Data.Audit;

/// <summary>
/// Interface for property-level audit tracking.
/// </summary>
public interface IPropertyAudit
{
    /// <summary>
    /// Gets the name of the property that changed.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets the old value of the property before the change.
    /// </summary>
    string? OldValue { get; }

    /// <summary>
    /// Gets the new value of the property after the change.
    /// </summary>
    string? NewValue { get; }
}
