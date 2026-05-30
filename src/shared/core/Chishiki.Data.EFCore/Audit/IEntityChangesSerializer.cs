// -----------------------------------------------------------------------------
// File:        IEntityChangesSerializer.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for serializing entity changes to audit records.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Abstractions;
using Chishiki.Data.Audit;

namespace Chishiki.Data.EFCore.Audit;

/// <summary>Serializer interface for handling and persisting entity audit changes.</summary>
public interface IEntityChangesSerializer
{
    /// <summary>Handles and persists a collection of audit entity changes asynchronously. .</summary>
    /// <typeparam name="TEntityAudit">The audit entity type implementing IEntityAudit.</typeparam>
    /// <param name="unitOfWork">The unit of work to use for persisting audit records.</param>
    /// <param name="changes">The collection of audit entities to persist.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleChangesAsync<TEntityAudit>(IUnitOfWork unitOfWork,
        IEnumerable<TEntityAudit> changes,
        CancellationToken cancellationToken = default)
        where TEntityAudit : class, IEntityAudit, new();
}
