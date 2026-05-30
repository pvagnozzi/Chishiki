// -----------------------------------------------------------------------------
// File:        IVideoSource.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for a disposable source capable of capturing image frames.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction;

/// <summary>Defines a disposable source capable of capturing image frames.</summary>
public interface IVideoSource : IDisposable
{
    /// <summary>Captures the next frame from the video source.</summary>
    /// <returns>The captured <see cref="IImage"/> frame.</returns>
    IImage GetFrame();
}
