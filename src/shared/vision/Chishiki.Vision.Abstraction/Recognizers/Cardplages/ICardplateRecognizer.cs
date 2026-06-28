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

namespace Chishiki.Vision.Abstraction.Recognizers.Cardplages;

/// <summary>Defines a recognizer that extracts cardplate text from a cropped image region.</summary>
public interface ICardplateRecognizer : IRecognizer<CardplateRecognizerOptions, CardplateRecognitionResult>;
