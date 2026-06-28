// -----------------------------------------------------------------------------
// File:        CardplateDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for cardplate detectors.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Cardplates;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Detectors.Cardplates;

/// <summary>Base class for typed cardplate detectors.</summary>
/// <param name="options">The detector options.</param>
/// <param name="logger">The logger for the detector.</param>
public abstract class CardplateDetector(CardplateDetectorOptions options, ILogger logger)
    : Detector<CardplateDetection, CardplateDetectorOptions>(options, logger), ICardplateDetector
{
    /// <summary>Gets the strongly typed options for this detector.</summary>
    public new CardplateDetectorOptions Options => (CardplateDetectorOptions)base.Options;
    CardplateDetectorOptions IDetector<CardplateDetection, CardplateDetectorOptions>.Options => Options;
}
