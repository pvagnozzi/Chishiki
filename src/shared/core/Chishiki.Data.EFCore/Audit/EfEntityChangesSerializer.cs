// -----------------------------------------------------------------------------
// File:        EfEntityChangesSerializer.cs
// Author:      Piergiorgio Vagnozzi
// Description: EF Core entity changes serializer for audit logging.
// Created:     2026-05-04
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Abstractions;
using Chishiki.Data.Audit;

namespace Chishiki.Data.EFCore.Audit;

/// <summary>
/// Entity Framework entity changes serializer that persists entity changes to an audit table.
/// </summary>
public class EfEntityChangesSerializer : IEntityChangesSerializer
{
    /// <summary>
    /// Handles and persists entity changes to the audit table asynchronously.
    /// </summary>
    /// <typeparam name="TEntityAudit">The audit entity type implementing IEntityAudit.</typeparam>
    /// <param name="unitOfWork">The unit of work to use for persisting audit records.</param>
    /// <param name="changes">The collection of audit entities to persist.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleChangesAsync<TEntityAudit>(IUnitOfWork unitOfWork,
        IEnumerable<TEntityAudit> changes,
        CancellationToken cancellationToken = default)
        where TEntityAudit : class, IEntityAudit, new()
    {
        using var repository = unitOfWork.GetRepository<Guid, TEntityAudit>();
        await repository.AddRangeAsync(changes, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
