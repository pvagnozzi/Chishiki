// -----------------------------------------------------------------------------
// File:        Size.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents a size within a frame.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Models;

/// <summary>Represents a size within a frame.</summary>
/// <param name="Width">Gets the width of the size.</param>
/// <param name="Height">Gets the height of the size.</param>
public record Size(int Width = 0, int Height = 0);
