// -----------------------------------------------------------------------------
// File:        FaceDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Base class for face detectors.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Faces;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Detectors.Faces;

/// <summary>Base class for typed face detectors.</summary>
/// <param name="options">The detector options.</param>
/// <param name="logger">The logger for the detector.</param>
public abstract class FaceDetector(FaceDetectorOptions options, ILogger logger)
    : Detector<FaceDetection, FaceDetectorOptions>(options, logger), IFaceDetector;
