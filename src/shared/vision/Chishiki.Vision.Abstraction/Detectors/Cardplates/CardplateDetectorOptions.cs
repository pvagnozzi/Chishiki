// -----------------------------------------------------------------------------
// File:        CardplateDetectorOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options for cardplate detection algorithms.
// Created:     2026-06-07
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Cardplates;

/// <summary>Defines the common configuration used by cardplate detection algorithms.</summary>
/// <remarks>
/// All tuning parameters (<c>MinContourArea</c>, <c>MinAspectRatio</c>, <c>MaxAspectRatio</c>,
/// <c>MinRectangularity</c>, <c>CandidatePaddingFactor</c>, <c>MaxCandidates</c>) are inherited
/// from <see cref="DetectorOptions"/> with the same default values and do not need to be redeclared.
/// </remarks>
public record CardplateDetectorOptions : DetectorOptions;
