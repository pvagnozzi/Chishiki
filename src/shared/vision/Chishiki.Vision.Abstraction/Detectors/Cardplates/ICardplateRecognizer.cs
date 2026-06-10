// -----------------------------------------------------------------------------
// File:        ICardplateRecognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines a recognizer for extracting cardplate text from cropped image regions.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Defines a recognizer that extracts cardplate text from a cropped image region.</summary>
public interface ICardplateRecognizer : IDisposable
{
    /// <summary>Gets the strongly typed options for the recognizer.</summary>
    CardplateRecognizerOptions Options { get; }

    /// <summary>Recognizes cardplate text from the supplied image region.</summary>
    /// <param name="image">The cropped image region that should contain a cardplate.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A recognition result describing the extracted text, when available.</returns>
    Task<CardplateRecognitionResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default);
}
