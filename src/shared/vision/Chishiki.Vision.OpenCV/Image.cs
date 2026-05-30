// -----------------------------------------------------------------------------
// File:        Image.cs
// Author:      Piergiorgio Vagnozzi
// Description: OpenCV Mat-backed implementation of IImage for use in the vision pipeline.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using OpenCvSharp;

namespace Chishiki.Vision.OpenCV;

/// <summary>OpenCV <see cref="Mat"/>-backed implementation of <see cref="IImage"/>.</summary>
public class Image(Mat imageData) : Disposable, IImage
{
    /// <summary>Initialises an empty <see cref="Image"/> backed by a new empty <see cref="Mat"/>.</summary>
    public Image() : this(new Mat())
    {
    }

    /// <inheritdoc/>
    public byte[] ImageData { get; } = imageData.ToBytes();

    /// <inheritdoc/>
    public bool IsEmpty() => Mat.Empty();

    /// <summary>Gets the underlying OpenCV <see cref="Mat"/> buffer.</summary>
    protected internal Mat Mat { get; } = imageData;

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        Mat.Dispose();
        base.DisposeManaged();
    }
}

