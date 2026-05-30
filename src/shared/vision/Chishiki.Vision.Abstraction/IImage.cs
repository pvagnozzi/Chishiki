// -----------------------------------------------------------------------------
// File:        IImage.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for an image payload used throughout the vision pipeline.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction;

/// <summary>Defines an image payload used throughout the vision pipeline.</summary>
public interface IImage : IDisposable
{
    /// <summary>Gets the encoded image data for this image.</summary>
    byte[] ImageData { get; }

    /// <summary>Determines whether this image contains any pixel data.</summary>
    /// <returns><see langword="true"/> when the image is empty; otherwise, <see langword="false"/>.</returns>
    bool IsEmpty();
}
