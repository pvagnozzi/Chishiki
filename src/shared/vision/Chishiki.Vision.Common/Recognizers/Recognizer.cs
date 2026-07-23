// -----------------------------------------------------------------------------
// File:        Recognizer.cs
// Author:      Piergiorgio Vagnozzi
// Description: Recognizer base class.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Recognizers;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Recognizers;

/// <summary>
/// Base class for stateful recognizers that analyze sequential image frames. Concrete implementations should inherit from this class and implement the abstract members to provide specific recognition functionality.
/// </summary>
/// <typeparam name="TOptions">The type of the recognizer options.</typeparam>
/// <typeparam name="TResult">The type of the recognition result.</typeparam>
/// <param name="options">The recognizer options.</param>
/// <param name="logger">The logger instance.</param>
public abstract class Recognizer<TOptions, TResult>(TOptions options, ILogger logger) :
    Disposable(logger), IRecognizer<TOptions, TResult>
    where TOptions : RecognizerOptions
    where TResult : RecognitionResult
{
    /// <summary>
    /// Gets the recognizer options.
    /// </summary>
    public TOptions Options { get; init; } = options;

    /// <summary>
    /// Recognizes the given image and returns the recognition result.
    /// </summary>
    /// <param name="image">The image to be recognized.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous recognition operation, containing the recognition result.</returns>
    public abstract Task<TResult> RecognizeAsync(IImage image, CancellationToken cancellationToken = default);
}
