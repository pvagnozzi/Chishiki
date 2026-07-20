// -----------------------------------------------------------------------------
// File:        FaceRecognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for face recognizers.
// Created:     2026-06-07
// Modified:    2026-07-16
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Recognizers;
using Chishiki.Vision.Common.Recognizers;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Detectors.Faces;

/// <summary>Base class for face recognizers.</summary>
/// <param name="options">Recognizer configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public abstract class FaceRecognizer(RecognizerOptions options, ILogger logger)
    : Recognizer<RecognizerOptions, RecognitionResult>(options, logger)
{
    /// <inheritdoc/>
    public abstract override Task<RecognitionResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default);
}
