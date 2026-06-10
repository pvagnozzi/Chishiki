// -----------------------------------------------------------------------------
// File:        IFaceRecognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines a recognizer for identifying faces from cropped image regions.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Defines a recognizer that identifies faces from a cropped image region.</summary>
public interface IFaceRecognizer : IDisposable
{
    /// <summary>Gets the strongly typed options for the recognizer.</summary>
    FaceRecognizerOptions Options { get; }

    /// <summary>Recognizes a face identity from the supplied image region.</summary>
    /// <param name="image">The cropped image region that should contain a face.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A recognition result describing the identified face, when available.</returns>
    Task<FaceRecognitionResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default);
}
