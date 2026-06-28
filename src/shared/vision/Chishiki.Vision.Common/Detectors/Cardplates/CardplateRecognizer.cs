// -----------------------------------------------------------------------------
// File:        CardplateRecognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for cardplate recognizers.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Recognizers.Cardplages;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Detectors.Cardplates;

/// <summary>Base class for cardplate recognizers.</summary>
/// <param name="options">Recognizer configuration options.</param>
/// <param name="logger">Logger used for diagnostics.</param>
public abstract class CardplateRecognizer(CardplateRecognizerOptions options, ILogger logger) : Disposable(logger), ICardplateRecognizer
{
    /// <inheritdoc/>
    public CardplateRecognizerOptions Options { get; } = options;

    /// <inheritdoc/>
    public abstract Task<CardplateRecognitionResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default);
}
