// -----------------------------------------------------------------------------
// File:        Detector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Detector base class.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Detectors;

/// <summary>
/// Base class for stateful detectors that analyse sequential video frames. Concrete implementations should inherit from this class and implement the abstract members to provide specific detection functionality.
/// </summary>
/// <param name="logger">Logger instance.</param>
/// <typeparam name="TDetection">Detection type.</typeparam>
/// <typeparam name="TOptions">Options type.</typeparam>
public abstract class Detector<TDetection, TOptions>(TOptions options, ILogger logger) :
    Disposable(logger), IDetector<TDetection>
    where TDetection : Detection
    where TOptions : DetectorOptions
{
    /// <summary> Gets the detector options. </summary>
    public TOptions Options { get; } = options;

    /// <summary>
    /// Analyses the supplied <paramref name="frame"/> against the accumulated background model and returns a <see cref="TResult"/> with annotated image and detected regions.
    /// </summary>
    /// <param name="frame">Source frame.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detection result.</returns>
    public abstract Task<DetectionResult<TDetection>> DetectAsync(IImage frame, CancellationToken cancellationToken = default);

    /// <summary> Resets the history. </summary>
    public abstract void Reset();
}
