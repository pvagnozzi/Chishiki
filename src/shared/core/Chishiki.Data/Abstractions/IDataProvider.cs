// -----------------------------------------------------------------------------
// File:        IDataProvider.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for database provider abstraction.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.Data.Abstractions;

/// <summary>Data provider interface.</summary>
public interface IDataProvider : IDisposable
{
    /// <summary>Finds the first record asynchronously. .</summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="parameters">Parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">Record type.</typeparam>
    /// <returns>Record or null i not found.</returns>
    Task<T?> FirstOrDefaultAsync<T>(string sql, object? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Finds a single record asynchronously. .</summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="parameters">Parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="T">Record type.</typeparam>
    /// <returns>Record or null i not found.</returns>
    Task<T?> SingleOrDefaultAsync<T>(string sql, object? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Queries the asynchronous. .</summary>
    /// <typeparam name="T">Record type.</typeparam>
    /// <param name="sql">The SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Query result.</returns>
    Task<IEnumerable<T>> ListAsync<T>(string sql, object? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>Executes the asynchronous. .</summary>
    /// <param name="sql">The SQL.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Affected records.</returns>
    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);
}


