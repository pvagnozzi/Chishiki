// -----------------------------------------------------------------------------
// File:        Detection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a rectangular region within a frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Models;

namespace Chishiki.Vision.Abstraction.Detectors;

/// <summary>Represents a rectangular region within a frame.</summary>
public record Detection
{
    /// <summary>
    /// Gets the detection rect.
    /// </summary>
    public Rect Rect { get; }

    /// <summary>
    /// Gets the area of the region, calculated as width multiplied by height.
    /// </summary>
    public double Area { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Detection"/> record with the specified point and size.
    /// </summary>
    /// <param name="rect">The detection rectangle.</param>
    /// <param name="area">The area of the region. If not provided, it will be calculated as width multiplied by height.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once MemberCanBeProtected.Global
    public Detection(Rect rect, double area = -1)
    {
        Rect = rect;
        Area = area >= 0 ? area : rect.Width * rect.Height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Detection"/> record with the specified point and size.
    /// </summary>
    /// <param name="point">The top-left corner of the region.</param>
    /// <param name="size">The size of the region.</param>
    /// <param name="area">The area of the region. If not provided, it will be calculated as width multiplied by height.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once MemberCanBeProtected.Global
    public Detection(Point point, Size size, double area = -1) : this(new Rect(point, size), area)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> record with the specified coordinates and dimensions.
    /// </summary>
    /// <param name="x">X-coordinate of the top-left corner of the region.</param>
    /// <param name="y">Y-coordinate of the top-left corner of the region.</param>
    /// <param name="width">Width of the region.</param>
    /// <param name="height">Height of the region.</param>
    /// <param name="area">The area of the region. If not provided, it will be calculated as width multiplied by height.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public Detection(int x, int y, int width, int height, double area = -1) : this(new Point(x, y), new Size(width, height), area)
    {
    }
}
