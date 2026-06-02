// -----------------------------------------------------------------------------
// File:        ImageProcessor.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for image processors providing a common foundation for all image processing components.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Processing;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Processing;

/// <summary>
/// Base class for image processors. This abstract class provides a common foundation for all image processing components that implement the <see cref="IImageProcessor"/> interface. It inherits from the <see cref="Disposable"/> base class, allowing derived classes to manage resources effectively while providing a consistent logging mechanism through the injected <see cref="ILogger"/> instance. Concrete implementations of this class should override the <see cref="ProcessAsync"/> method to perform specific image processing tasks, such as filtering, enhancement, or transformation, on the input image and return the processed result.
/// </summary>
/// <param name="logger">The logger instance used for logging within the image processor.</param>
public abstract class ImageProcessor(ILogger<ImageProcessor> logger) : Disposable(logger), IImageProcessor
{
    /// <inheritdoc/>
    public abstract Task<IImage> ProcessAsync(IImage image, CancellationToken cancellationToken = default);
}
