// -----------------------------------------------------------------------------
// File:        IRecognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines a recognizer for identifying faces from cropped image regions.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Recognizers;

/// <summary>
/// Defines a recognizer that identifies objects from a cropped image region, using strongly typed options and recognition results.
/// </summary>
/// <typeparam name="TOptions">The type of the recognizer options.</typeparam>
/// <typeparam name="TResult">The type of the recognition result.</typeparam>
public interface IRecognizer<TOptions, TResult> : IDisposable
    where TOptions : RecognizerOptions
    where TResult : RecognitionResult
{
    /// <summary>Gets the strongly typed options for the recognizer.</summary>
    TOptions Options { get; }

    /// <summary>Recognizes an object identity from the supplied image region.</summary>
    /// <param name="image">The cropped image region that should contain an object.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A recognition result describing the identified object, when available.</returns>
    Task<TResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default);
}

