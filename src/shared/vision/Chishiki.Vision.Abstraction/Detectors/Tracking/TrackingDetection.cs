// -----------------------------------------------------------------------------
// File:        TrackingDetection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a tracked region within a video frame.
// Created:     2026-06-06
// Modified:    2026-06-06
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Models;

namespace Chishiki.Vision.Abstraction.Detectors.Tracking;

/// <summary>Represents a tracked region within a video frame.</summary>
public record TrackingDetection : Detection
{
    /// <summary>Initializes a new instance of the <see cref="TrackingDetection"/> record with the specified rectangle.</summary>
    /// <param name="rect">The tracked rectangle.</param>
    /// <param name="area">The area of the tracked region. If not provided, it is calculated from the rectangle dimensions.</param>
    public TrackingDetection(Rect rect, double area = -1) : base(rect, area)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TrackingDetection"/> record with the specified point and size.</summary>
    /// <param name="point">The top-left corner of the tracked region.</param>
    /// <param name="size">The size of the tracked region.</param>
    /// <param name="area">The area of the tracked region. If not provided, it is calculated from the size.</param>
    public TrackingDetection(Point point, Size size, double area = -1) : base(point, size, area)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="TrackingDetection"/> record with the specified coordinates and dimensions.</summary>
    /// <param name="x">The x-coordinate of the top-left corner.</param>
    /// <param name="y">The y-coordinate of the top-left corner.</param>
    /// <param name="width">The width of the tracked region.</param>
    /// <param name="height">The height of the tracked region.</param>
    /// <param name="area">The area of the tracked region. If not provided, it is calculated from width and height.</param>
    public TrackingDetection(int x, int y, int width, int height, double area = -1) : base(x, y, width, height, area)
    {
    }
}
