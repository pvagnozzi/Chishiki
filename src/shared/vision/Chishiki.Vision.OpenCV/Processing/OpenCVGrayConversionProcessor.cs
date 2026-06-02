// -----------------------------------------------------------------------------
// File:        OpenGrayConversionProcessor.cs
// Author:      Piergiorgio Vagnozzi
// Description: An image processor that converts the input image to grayscale using OpenCV. This processor takes a color image as input and produces a single-channel grayscale image as output, which can be useful for various computer vision tasks that require intensity information rather than color information.
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
/// An image processor that converts the input image to grayscale using OpenCV. This processor takes a color image as input and produces a single-channel grayscale image as output, which can be useful for various computer vision tasks that require intensity information rather than color information.
/// </summary>
/// <param name="logger">The logger instance for logging.</param>
public class OpenGrayConversionProcessor(ILogger<ImageProcessor> logger) : OpenCVImageProcessor(logger)
{
    /// <summary>
    /// Processes the input image by converting it to grayscale. The grayscale image is returned as a new Mat object.
    /// </summary>
    /// <param name="image">The input image to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    protected override Task<Mat> ProcessImageAsync(Mat image, CancellationToken cancellationToken = default)
    {
        Mat gray = new();
        Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
        return Task.FromResult(gray);
    }
}
