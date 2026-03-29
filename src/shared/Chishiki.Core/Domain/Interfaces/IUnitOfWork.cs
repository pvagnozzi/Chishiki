// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Core.Domain.Interfaces;

/// <summary>Coordinates writing of changes to the underlying data store in a single transaction.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes to the data store.</summary>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The number of state entries written to the store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
