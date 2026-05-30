// -----------------------------------------------------------------------------
// File:        EntityAudit.cs
// Author:      Piergiorgio Vagnozzi
// Description: Audit entity that records changes to other entities with user ID, entity name, ID, action, and property changes.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki;
using Chishiki.Data.Audit;
using Chishiki.Data.EFCore.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chishiki.Data.EFCore.Audit;

/// <summary>Audit entity that records all changes (create, update, delete) to other entities, including the user who made the change, the entity identifier, and all property changes.</summary>
/// <param name="userId">The ID of the user who made the change.</param>
/// <param name="entityName">The fully qualified name of the entity type that was changed.</param>
/// <param name="entityId">The primary key value of the entity that was changed.</param>
/// <param name="changeAction">The type of change (Inserted, Updated, or Deleted).</param>
/// <param name="properties">A JSON-serialized string containing all property changes.</param>
public class EntityAudit(Guid userId, string entityName, string entityId, ChangeAction changeAction, string properties)
    : EFGuidEntity, IEntityAudit
{
    /// <summary>Parameterless constructor for EF Core and serialization frameworks. .</summary>
    protected EntityAudit() : this(Guid.Empty, string.Empty, string.Empty, ChangeAction.Inserted, string.Empty)
    {
    }

    /// <summary>Gets the ID of the user who made the change. .</summary>
    [Required]
    public Guid UserId { get; init; } = userId;

    /// <summary>Gets the fully qualified name of the entity type that was changed. .</summary>
    [Required]
    [MaxLength(256)]
    public string EntityName { get; init; } = entityName;

    /// <summary>Gets the primary key value of the entity that was changed. .</summary>
    [Required]
    [MaxLength(64)]
    public string EntityId { get; init; } = entityId;

    /// <summary>Gets the type of change that occurred (Inserted, Updated, or Deleted). .</summary>
    [Required]
    public ChangeAction Action { get; init; } = changeAction;

    /// <summary>Gets the JSON-serialized string containing all property changes with old and new values. .</summary>
    public string Properties { get; init; } = properties;
}

/// <summary>Extension methods for EntityAudit conversion and serialization.</summary>
public static class EntityAuditExtensions
{
    /// <summary>Converts an EntityChange to an EntityAudit audit record for the specified user. .</summary>
    /// <typeparam name="TEntityAudit">The audit entity type implementing IEntityAudit.</typeparam>
    /// <param name="entityChange">The entity change to convert.</param>
    /// <param name="userId">The ID of the user who made the change.</param>
    /// <returns>A new audit entity instance with the converted data.</returns>
    public static TEntityAudit ToEntityAudit<TEntityAudit>(this EntityChange entityChange, Guid userId)
        where TEntityAudit : class, IEntityAudit, new() =>
        new()
        {
            UserId = userId,
            EntityName = entityChange.EntityName,
            EntityId = entityChange.EntityId.ToString()!,
            Action = entityChange.Operation,
            Properties = entityChange.Properties.Serialize()
        };

    /// <summary>Deserializes a JSON string into a collection of PropertyAudit records. .</summary>
    /// <param name="properties">The JSON-serialized property changes string.</param>
    /// <returns>An array of PropertyAudit records, or an empty array if deserialization fails.</returns>
    public static IPropertyAudit[] DeserializePropertyAudit(this string properties) =>
        JsonSerializer.Deserialize<PropertyAuditJsonSerialization[]>(properties)
            ?.Select(x => new PropertyAudit(x.PropertyName, x.OldValue, x.NewValue)).Cast<IPropertyAudit>().ToArray() ??
        [];

    /// <summary>Serializes a collection of PropertyChange objects to a JSON string. .</summary>
    /// <param name="properties">The collection of property changes to serialize.</param>
    /// <returns>A JSON-serialized string representation of the property changes.</returns>
    private static string Serialize(this IEnumerable<PropertyChange> properties) =>
        JsonSerializer.Serialize(properties.Select(x =>
            new PropertyAuditJsonSerialization(x.PropertyName, x.OldValue?.ToString(), x.NewValue?.ToString())));

    /// <summary>Internal record for JSON serialization of property audit data. .</summary>
    /// <param name="PropertyName">The name of the property that was changed.</param>
    /// <param name="OldValue">The value before the change.</param>
    /// <param name="NewValue">The value after the change.</param>
    private record PropertyAuditJsonSerialization(
        [property: JsonPropertyName("PropertyName")]
        string PropertyName,
        [property: JsonPropertyName("oldValue")]
        string? OldValue,
        [property: JsonPropertyName("newValue")]
        string? NewValue);
}
