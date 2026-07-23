// -----------------------------------------------------------------------------
// File:        AsyncEnumerableTestExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Provides small test-only helpers for materialising async streams.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.SemanticKernel.Tests;

/// <summary>Provides test-only helpers for async enumeration.</summary>
internal static class AsyncEnumerableTestExtensions
{
    /// <summary>Materialises an async stream into a list.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source stream.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task that completes with all elements from <paramref name="source"/>.</returns>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            results.Add(item);
        }

        return results;
    }
}
