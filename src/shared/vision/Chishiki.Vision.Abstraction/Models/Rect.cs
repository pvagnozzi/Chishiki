// -----------------------------------------------------------------------------
// File:        Rect.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a rectangular region within a frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Models;

/// <summary>Represents a rectangular region within a frame.</summary>
public record Rect
{
    /// <summary>
    /// Gets the top-left corner of the region, represented as a <see cref="Point"/> structure.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Point Point { get; init; }

    /// <summary>
    /// Gets the size of the region, which includes its width and height.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public Size Size { get; init; }

    /// <summary>
    /// Gets the X-coordinate of the top-left corner of the region, derived from the Point property.
    /// </summary>
    public int X => Point.X;

    /// <summary>
    /// Gets the Y-coordinate of the top-left corner of the region, derived from the Point property.
    /// </summary>
    public int Y => Point.Y;

    /// <summary>
    /// Gets the width of the region, derived from the Size property.
    /// </summary>
    public int Width => Size.Width;

    /// <summary>
    /// Gets the height of the region, derived from the Size property.
    /// </summary>
    public int Height => Size.Height;

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> record with the specified point and size.
    /// </summary>
    public Rect() : this(new Point(), new Size())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> record with the specified point and size.
    /// </summary>
    /// <param name="point">The top-left corner of the region.</param>
    /// <param name="size">The size of the region.</param>
    public Rect(Point point, Size size)
    {
        Point = point;
        Size = size;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> record with the specified coordinates and dimensions.
    /// </summary>
    /// <param name="x">X-coordinate of the top-left corner of the region.</param>
    /// <param name="y">Y-coordinate of the top-left corner of the region.</param>
    /// <param name="width">Width of the region.</param>
    /// <param name="height">Height of the region.</param>
    public Rect(int x, int y, int width, int height) : this(new Point(x, y), new Size(width, height))
    {
    }
}
