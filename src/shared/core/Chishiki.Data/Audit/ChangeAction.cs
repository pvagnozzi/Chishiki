// Modified:    2026-05-04

namespace Chishiki.Data.Audit;

/// <summary>
/// Enumeration of change actions for entity audit tracking.
/// </summary>
public enum ChangeAction
{
    /// <summary>Entity was inserted.</summary>
    Inserted,

    /// <summary>Entity was updated.</summary>
    Updated,

    /// <summary>Entity was deleted.</summary>
    Deleted
}
