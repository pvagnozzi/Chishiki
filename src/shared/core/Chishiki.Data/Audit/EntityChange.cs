// Modified:    2026-05-04

namespace Chishiki.Data.Audit;

/// <summary>
/// Record representing an entity change captured in an audit trail.
/// </summary>
/// <param name="EntityName">The name of the entity type that changed.</param>
/// <param name="EntityId">The unique identifier of the changed entity.</param>
/// <param name="Operation">The type of change action (insert, update, or delete).</param>
/// <param name="Properties">The collection of property-level changes.</param>
public record EntityChange(
    string EntityName,
    object EntityId,
    ChangeAction Operation,
    PropertyChange[] Properties)
{
    /// <summary>
    /// Gets the timestamp when the change occurred.
    /// </summary>
    public DateTimeOffset TimeStamp { get; init; } = DateTimeOffset.UtcNow;
}
