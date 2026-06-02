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
/// <typeparam name="TResult">Result type.</typeparam>
/// <typeparam name="TDetection">Detection type.</typeparam>
public abstract class Detector<TResult, TDetection>(DetectorOptions options, ILogger logger) :
    Disposable(logger), IDetector<TResult, TDetection>
    where TResult : DetectionResult<TDetection>
    where TDetection : Detection
{
    /// <summary> Gets the detector options. </summary>
    public DetectorOptions Options { get; } = options;

    /// <summary>
    /// Analyses the supplied <paramref name="frame"/> against the accumulated background model and returns a <see cref="TResult"/> with annotated image and detected regions.
    /// </summary>
    /// <param name="frame">Source frame.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detection result.</returns>
    public abstract Task<TResult> DetectAsync(IImage frame, CancellationToken cancellationToken = default);

    /// <summary> Resets the history. </summary>
    public abstract void Reset();
}
