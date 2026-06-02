// -----------------------------------------------------------------------------
// File:        Color.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a point within a frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Models;

/// <summary>
/// Represents a color using RGB values. Each component (Red, Green, Blue) is an integer typically in the range 0-255.
/// </summary>
/// <param name="R">Red value.</param>
/// <param name="G">Green value.</param>
/// <param name="B">Blue value.</param>
// ReSharper disable once ClassNeverInstantiated.Global
public record Color(int R, int G, int B)
{
    /// <summary>
    /// Gets the red value.
    /// </summary>
    public int R { get; } = R;

    /// <summary>
    /// Gets the green value.
    /// </summary>
    public int G { get; } = G;

    /// <summary>
    /// Gets the blue value.
    /// </summary>
    public int B { get; } = B;
}
