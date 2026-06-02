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

namespace Chishiki.Vision.Common;

/// <summary>Base implementation of <see cref="IImage"/>.</summary>
public abstract class Image : Disposable, IImage
{
    /// <inheritdoc/>
    public abstract byte[] ImageData { get; }

    /// <inheritdoc/>
    public abstract bool IsEmpty();
}

