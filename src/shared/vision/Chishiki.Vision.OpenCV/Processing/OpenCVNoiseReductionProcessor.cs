// -----------------------------------------------------------------------------
// File:        OpenCVNoiseReductionProcessor.cs
// Author:      Piergiorgio Vagnozzi
// Description: An image processor that applies Gaussian blur to reduce noise in the image using OpenCV.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Common.Processing;
using Microsoft.Extensions.Logging;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV.Processing;

/// <summary>
/// An image processor that applies Gaussian blur to reduce noise in the image using OpenCV.
/// </summary>
/// <param name="gaussianKernelSize">The size of the Gaussian kernel used for blurring.</param>
/// <param name="logger">The logger instance for logging.</param>
public class OpenCVNoiseReductionProcessor(int gaussianKernelSize, ILogger<ImageProcessor> logger) : OpenCVImageProcessor(logger)
{
    /// <summary>
    /// The size of the Gaussian kernel used for blurring. It is initialized in the constructor and remains constant for the lifetime of the processor.
    /// </summary>
    private readonly Size _gaussianKernelSize = new(gaussianKernelSize, gaussianKernelSize);

    /// <summary>
    /// Processes the input image by applying a Gaussian blur to reduce noise. The blurred image is returned as a new Mat object.
    /// </summary>
    /// <param name="image">The input image to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    protected override Task<Mat> ProcessImageAsync(Mat image, CancellationToken cancellationToken = default)
    {
        Mat blurred = new();
        Cv2.GaussianBlur(image, blurred, _gaussianKernelSize, sigmaX: 0);
        return Task.FromResult(blurred);
    }
}
