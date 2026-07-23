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
    /// <summary>
    /// Converts an <see cref="IImage"/> to an OpenCV <see cref="Mat"/>. If the image is already an <see cref="OpenCVImage"/>, its underlying <see cref="Mat"/> is returned directly. Otherwise, a new <see cref="Mat"/> is created from the image's byte data. This allows for seamless integration of any <see cref="IImage"/> into OpenCV processing functions.
    /// </summary>
    /// <param name="image">The source image to convert.</param>
    /// <returns>An OpenCV <see cref="Mat"/> containing the same image data.</returns>
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

    /// <summary>
    /// Extension method to convert a <see cref="VisionRect"/> to an OpenCV <see cref="Rect"/>. This is used to convert bounding boxes from the vision pipeline into a format compatible with OpenCV functions.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="imageSize"></param>
    /// <param name="paddingFactor"></param>
    /// <returns></returns>
    public static Rect ExpandRect(this Rect rect, Size imageSize, double paddingFactor)
    {
        var paddingX = (int)Math.Round(rect.Width * Math.Max(0.0, paddingFactor));
        var paddingY = (int)Math.Round(rect.Height * Math.Max(0.0, paddingFactor));

        var x = Math.Max(0, rect.X - paddingX);
        var y = Math.Max(0, rect.Y - paddingY);
        var right = Math.Min(imageSize.Width, rect.Right + paddingX);
        var bottom = Math.Min(imageSize.Height, rect.Bottom + paddingY);

        return new Rect(x, y, Math.Max(1, right - x), Math.Max(1, bottom - y));
    }

    /// <summary>
    /// Calculates a detection score based on the aspect ratio and rectangularity of a candidate region. This is used to rank potential cardplate detections.
    /// </summary>
    /// <param name="aspectRatio">The aspect ratio of the candidate region.</param>
    /// <param name="rectangularity">The rectangularity of the candidate region.</param>
    /// <returns>A score between 0.0 and 1.0 indicating the likelihood of the region being a cardplate.</returns>
    public static float CalculateDetectionScore(this double aspectRatio, double rectangularity)
    {
        const double idealAspectRatio = 4.0;
        var aspectScore = 1.0 - Math.Min(1.0, Math.Abs(aspectRatio - idealAspectRatio) / idealAspectRatio);
        return (float)Math.Clamp((aspectScore + rectangularity) / 2.0, 0.0, 1.0);
    }

    /// <summary>
    /// Converts an OpenCV <see cref="Mat"/> to a grayscale image. If the image is already grayscale, a clone is returned.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <returns>A grayscale <see cref="Mat"/>.</returns>
    public static Mat ToGrayscale(this Mat image)
    {
        if (image.Channels() == 1)
        {
            return image.Clone();
        }

        var grayscale = new Mat();
        var conversionCode = image.Channels() switch
        {
            4 => ColorConversionCodes.BGRA2GRAY,
            _ => ColorConversionCodes.BGR2GRAY
        };

        Cv2.CvtColor(image, grayscale, conversionCode);
        return grayscale;
    }

    /// <summary>
    /// Ensures that the kernel size is odd, which is required for certain OpenCV operations like Gaussian blur.
    /// </summary>
    /// <param name="size">The desired kernel size.</param>
    /// <returns>An odd kernel size, adjusted if necessary.</returns>
    public static int EnsureOddKernel(this int size) => size <= 1 ? 1 : size % 2 == 0 ? size + 1 : size;
}
