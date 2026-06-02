// -----------------------------------------------------------------------------
// File:        OpenCVImageProcessor.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for OpenCV-based image processors.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Common.Processing;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV.Processing;

/// <summary>
/// Base class for OpenCV-based image processors. This abstract class provides a common foundation for all image processing components that utilize OpenCV for their operations. It inherits from the <see cref="ImageProcessor"/> base class, allowing derived classes to implement specific image processing algorithms while sharing common functionality and logging capabilities. The class defines an abstract method <see cref="ProcessImageAsync"/> that must be implemented by concrete subclasses to perform the actual image processing logic using OpenCV's Mat objects, enabling efficient manipulation and transformation of images within the processing pipeline.
/// </summary>
/// <param name="logger">The logger instance used for logging within the image processor.</param>
public abstract class OpenCVImageProcessor(ILogger<ImageProcessor> logger) : ImageProcessor(logger)
{
    /// <summary>
    /// Processes the input image asynchronously and returns the processed image. This method overrides the base class implementation to utilize OpenCV's Mat objects for image manipulation. It converts the input <see cref="IImage"/> to an OpenCV Mat, calls the abstract <see cref="ProcessImageAsync"/> method to perform the specific image processing logic defined in derived classes, and then wraps the resulting Mat back into an <see cref="IImage"/> implementation for further use in the processing pipeline.
    /// </summary>
    /// <param name="image">The input image to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The processed image.</returns>
    public override async Task<IImage> ProcessAsync(IImage image, CancellationToken cancellationToken = default) =>
        new OpenCVImage(await ProcessImageAsync(image.ToMat(), cancellationToken));

    /// <summary>
    /// Processes the input OpenCV Mat image asynchronously and returns the processed Mat. This abstract method must be implemented by derived classes to perform specific image processing tasks using OpenCV's Mat objects. The implementation should take the input Mat, apply the desired transformations or filters, and return a new Mat containing the processed image data. The method also accepts a cancellation token for cooperative cancellation of the processing operation.
    /// </summary>
    /// <param name="image">The input OpenCV Mat image to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The processed OpenCV Mat image.</returns>
    public Task<Mat> ProcessAsync(Mat image, CancellationToken cancellationToken = default) => ProcessImageAsync(image, cancellationToken);

    /// <summary>
    /// Processes the input OpenCV Mat image asynchronously and returns the processed Mat. This abstract method must be implemented by derived classes to perform specific image processing tasks using OpenCV's Mat objects. The implementation should take the input Mat, apply the desired transformations or filters, and return a new Mat containing the processed image data. The method also accepts a cancellation token for cooperative cancellation of the processing operation.
    /// </summary>
    /// <param name="image">The input OpenCV Mat image to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The processed OpenCV Mat image.</returns>
    protected abstract Task<Mat> ProcessImageAsync(Mat image, CancellationToken cancellationToken = default);
}
