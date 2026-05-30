// -----------------------------------------------------------------------------
// File:        IImageFilter.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for a single-step image processing filter in the vision pipeline.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction;

/// <summary>Defines a single-step image processing filter applied within a vision pipeline.</summary>
public interface IImageFilter
{
    /// <summary>Applies this filter to the supplied <paramref name="image"/> and returns the processed result.</summary>
    /// <param name="image">The source image to process.</param>
    /// <returns>A new <see cref="IImage"/> containing the filtered output.</returns>
    IImage Apply(IImage image);
}

