// -----------------------------------------------------------------------------
// File:        ICardplateDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines a detector for vehicle cardplates in image frames.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Defines a detector that locates vehicle cardplates and optionally enriches detections with recognized text.</summary>
public interface ICardplateDetector : IDetector<CardplateDetectionResult, CardplateDetection>
{
    /// <summary>Gets the strongly typed options for the cardplate detector.</summary>
    new CardplateDetectorOptions Options { get; }
}
