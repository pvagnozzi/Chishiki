// Modified:    2026-05-04

using Chishiki.Data.Models;

namespace Chishiki.Data.Audit;

/// <summary>
/// Interface for entity-level audit trail tracking with date tracking and user correlation.
/// </summary>
public interface IEntityAudit : IEntityWithDates<Guid>
{
    /// <summary>
    /// Gets the ID of the user who made the change.
    /// </summary>
    Guid UserId { get; init; }

    /// <summary>
    /// Gets the ID of the audited entity.
    /// </summary>
    string EntityId { get; init; }

    /// <summary>
    /// Gets the name of the audited entity type.
    /// </summary>
    string EntityName { get; init; }

    /// <summary>
    /// Gets the action performed on the entity (insert, update, or delete).
    /// </summary>
    ChangeAction Action { get; init; }

    /// <summary>
    /// Gets the serialized representation of property changes (typically JSON).
    /// </summary>
    string Properties { get; init; }
}
