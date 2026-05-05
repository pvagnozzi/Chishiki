// -----------------------------------------------------------------------------
// File:        PagedList.cs
// Author:      Piergiorgio Vagnozzi
// Description: Paginated list implementation with metadata and map extension methods.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki;

/// <summary>Represents a paginated list with items and pagination metadata.</summary>
/// <typeparam name="T">The type of items in the paginated list.</typeparam>
public record PagedList<T> : IPagedList<T>
{
    /// <summary>Initializes a new instance of the <see cref="PagedList{T}"/> record.</summary>
    /// <param name="items">Optional collection of items to include in this page.</param>
    /// <param name="totalCount">Total number of items across all pages. If null, defaults to the count of <paramref name="items"/>.</param>
    /// <param name="pageIndex">Zero-based index of the current page.</param>
    /// <param name="pageSize">Maximum number of items per page. If null, defaults to <paramref name="totalCount"/>.</param>
    public PagedList(IEnumerable<T>? items = null, int? totalCount = null, int pageIndex = 0, int? pageSize = null)
    {
        Items = items?.ToArray() ?? [];
        TotalCount = totalCount ?? Items.Count;
        PageSize = pageSize ?? TotalCount;
        PageIndex = pageIndex;
        TotalPages = PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <inheritdoc/>
    public IList<T> Items { get; }

    /// <inheritdoc/>
    public int PageIndex { get; }

    /// <inheritdoc/>
    public int TotalPages { get; }

    /// <inheritdoc/>
    public int PageSize { get; }

    /// <inheritdoc/>
    public int TotalCount { get; }
}

/// <summary>Extension methods for <see cref="IPagedList{T}"/> providing mapping and transformation operations.</summary>
public static class PageListExtensions
{
    /// <summary>Maps items in a paginated list to a new type using the specified transformation function.</summary>
    /// <typeparam name="TSource">The type of items in the source paginated list.</typeparam>
    /// <typeparam name="TDest">The type of items in the destination paginated list.</typeparam>
    /// <param name="source">The source paginated list to map.</param>
    /// <param name="map">The transformation function to apply to each item.</param>
    /// <returns>A new <see cref="IPagedList{T}"/> with transformed items but the same pagination metadata.</returns>
    public static IPagedList<TDest> Map<TSource, TDest>(this IPagedList<TSource> source, Func<TSource, TDest> map) =>
        new PagedList<TDest>(source.Items.Select(map), source.TotalCount, source.PageIndex, source.PageSize);
}
