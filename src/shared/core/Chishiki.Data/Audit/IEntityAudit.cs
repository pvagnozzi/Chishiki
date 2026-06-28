// -----------------------------------------------------------------------------
// File:        IEntityAudit.cs
// Author:      Piergiorgio Vagnozzi
// Description: Contract for persisted entity-level audit records with user and change metadata.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Models;

namespace Chishiki.Data.Audit;

/// <summary>Interface for entity-level audit trail tracking with date tracking and user correlation.</summary>
public interface IEntityAudit : IEntity<Guid>
{
    /// <summary>Gets the ID of the user who made the change. .</summary>
    Guid UserId { get; init; }

    /// <summary>Gets the ID of the audited entity. .</summary>
    string EntityId { get; init; }

    /// <summary>Gets the name of the audited entity type. .</summary>
    string EntityName { get; init; }

    /// <summary>Gets the action performed on the entity (insert, update, or delete). .</summary>
    ChangeAction Action { get; init; }

    /// <summary>Gets the serialized representation of property changes (typically JSON). .</summary>
    string Properties { get; init; }
}
