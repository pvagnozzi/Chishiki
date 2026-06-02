// -----------------------------------------------------------------------------
// File:        MotionDetection.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a motion detection region within a video frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Models;

namespace Chishiki.Vision.Abstraction.Detectors.Motion;

/// <summary>Represents a detected motion region within a frame.</summary>
// ReSharper disable once ClassNeverInstantiated.Global
public record MotionDetection : Detection
{
    /// <summary>Initializes a new instance of the <see cref="MotionDetection"/> record with the specified rectangle.</summary>
    /// <param name="rect">The detection rectangle.</param>
    /// <param name="area">The area of the detection region. If not provided, it is calculated from the rectangle dimensions.</param>
    public MotionDetection(Rect rect, double area = -1) : base(rect, area)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="MotionDetection"/> record with the specified point and size.</summary>
    /// <param name="point">The top-left corner of the detection region.</param>
    /// <param name="size">The size of the detection region.</param>
    /// <param name="area">The area of the detection region. If not provided, it is calculated from the size.</param>
    public MotionDetection(Point point, Size size, double area = -1) : base(point, size, area)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="MotionDetection"/> record with the specified coordinates and dimensions.</summary>
    /// <param name="x">The x-coordinate of the top-left corner.</param>
    /// <param name="y">The y-coordinate of the top-left corner.</param>
    /// <param name="width">The width of the detection region.</param>
    /// <param name="height">The height of the detection region.</param>
    /// <param name="area">The area of the detection region. If not provided, it is calculated from width and height.</param>
    public MotionDetection(int x, int y, int width, int height, double area = -1) : base(x, y, width, height, area)
    {
    }
}
