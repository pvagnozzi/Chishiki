// -----------------------------------------------------------------------------
// File:        IImageProcessor.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for a single-step image processing filter in the vision pipeline.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Processing;

/// <summary>Defines a single-step image processing filter applied within a vision pipeline.</summary>
public interface IImageProcessor : IDisposable
{
    /// <summary>Processes the supplied <paramref name="image"/> using this processor and returns the processed result.</summary>
    /// <param name="image">The source image to process.</param>
    /// <returns>A new <see cref="IImage"/> containing the processed output.</returns>
    Task<IImage> ProcessAsync(IImage image, CancellationToken cancellationToken = default);
}
