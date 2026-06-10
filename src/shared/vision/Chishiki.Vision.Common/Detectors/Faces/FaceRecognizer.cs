// -----------------------------------------------------------------------------
// File:        FaceRecognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for face recognizers.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors.Faces;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Detectors.Faces;

/// <summary>Base class for face recognizers.</summary>
/// <param name="options">Recognizer configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public abstract class FaceRecognizer(FaceRecognizerOptions options, ILogger logger) : Disposable(logger), IFaceRecognizer
{
    /// <inheritdoc/>
    public FaceRecognizerOptions Options { get; } = options;

    /// <inheritdoc/>
    public abstract Task<FaceRecognitionResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default);
}
