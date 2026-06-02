// -----------------------------------------------------------------------------
// File:        OpenCVImage.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV Mat-backed implementation of IImage for use in the vision pipeline.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Common;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV;

/// <summary>OpenCV <see cref="Mat"/>-backed implementation of <see cref="IImage"/>.</summary>
public class OpenCVImage(Mat imageData) : Image
{
    /// <summary>Initialises an empty <see cref="OpenCVImage"/> backed by a new empty <see cref="Mat"/>.</summary>
    public OpenCVImage() : this(new Mat())
    {
    }

    /// <inheritdoc/>
    public override byte[] ImageData { get; } = imageData.ToBytes();

    /// <inheritdoc/>
    public override bool IsEmpty() => Mat.Empty();

    /// <summary>Gets the underlying OpenCV <see cref="Mat"/> buffer.</summary>
    protected internal Mat Mat { get; } = imageData;

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        Mat.Dispose();
        base.DisposeManaged();
    }
}

/// <summary>
/// OpenCV image extension methods for converting between <see cref="IImage"/> and <see cref="OpenCVImage"/>. Provides a convenient way to wrap raw image data in an OpenCV-compatible format for processing within the vision pipeline.
/// </summary>
public static class OpenCVImageExtensions
{
    public static Mat ToMat(this IImage image) => image is OpenCVImage openCvImage ? openCvImage.Mat : Mat.FromImageData(image.ImageData, ImreadModes.AnyColor);

    /// <summary>Converts an <see cref="IImage"/> to an <see cref="OpenCVImage"/> by wrapping its byte data in a new OpenCV <see cref="Mat"/>. This is used to convert frames from the video source into a format compatible with OpenCV processing.</summary>
    /// <param name="image">The source image to convert.</param>
    /// <returns>An <see cref="OpenCVImage"/> instance containing the same image data.</returns>
    public static OpenCVImage ToOpenCVImage(this IImage image) => image is OpenCVImage openCvImage ? openCvImage : new OpenCVImage(Mat.FromImageData(image.ImageData, ImreadModes.AnyColor));

    /// <summary>
    /// Converts an OpenCV <see cref="Mat"/> to an <see cref="IImage"/> by wrapping it in a new <see cref="OpenCVImage"/>. This allows OpenCV-processed images to be used in the vision pipeline wherever an <see cref="IImage"/> is expected.
    /// </summary>
    /// <param name="openCvImage">The OpenCV <see cref="Mat"/> to convert.</param>
    /// <returns>An <see cref="IImage"/> instance containing the same image data.</returns>
    public static IImage ToIImage(this Mat openCvImage) => new OpenCVImage(openCvImage);
}
