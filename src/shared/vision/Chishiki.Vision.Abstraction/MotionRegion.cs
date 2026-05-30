// -----------------------------------------------------------------------------
// File:        MotionRegion.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a rectangular region where motion has been detected.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction;

/// <summary>Represents a rectangular region within a frame where motion has been detected.</summary>
/// <param name="X">Gets the x-coordinate of the top-left corner of the motion region.</param>
/// <param name="Y">Gets the y-coordinate of the top-left corner of the motion region.</param>
/// <param name="Width">Gets the width in pixels of the motion region.</param>
/// <param name="Height">Gets the height in pixels of the motion region.</param>
/// <param name="Area">Gets the pixel area of the motion region.</param>
public record MotionRegion(int X, int Y, int Width, int Height, double Area);
