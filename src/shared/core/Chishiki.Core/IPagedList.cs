// -----------------------------------------------------------------------------
// File:        IPagedList.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for paginated list result sets with metadata about page count and total items.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki;

/// <summary>Represents a paginated list result set with metadata about page count and total items.</summary>
/// <typeparam name="T">The type of items in the paginated list.</typeparam>
public interface IPagedList<T>
{
    /// <summary>Gets the list of items on the current page.</summary>
    IList<T> Items { get; }

    /// <summary>Gets the total number of items across all pages.</summary>
    int TotalCount { get; }

    /// <summary>Gets the total number of pages available.</summary>
    int TotalPages { get; }

    /// <summary>Gets the maximum number of items per page.</summary>
    int PageSize { get; }

    /// <summary>Gets the zero-based index of the current page.</summary>
    int PageIndex { get; }
}
