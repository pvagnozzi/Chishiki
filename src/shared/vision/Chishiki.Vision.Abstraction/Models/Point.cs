// -----------------------------------------------------------------------------
// File:        Point.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a point within a frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Models;

/// <summary>Represents a point within a frame.</summary>
/// <param name="X">Gets the x-coordinate of the point.</param>
/// <param name="Y">Gets the y-coordinate of the point.</param>
public record Point(int X = 0, int Y = 0);
