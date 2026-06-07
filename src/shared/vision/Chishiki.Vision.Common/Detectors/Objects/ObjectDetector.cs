// -----------------------------------------------------------------------------
// File:        ObjectDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Object detector base class.
// Created:     2025-01-01
// Modified:    2025-01-01
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Objects;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.Common.Detectors.Objects;

/// <summary>
/// Object detector that composes multiple object detectors together. This allows for running multiple detectors on the same frame and aggregating their results.
/// </summary>
/// <param name="options">The options for the detector.</param>
/// <param name="logger">The logger for the detector.</param>
public abstract class ObjectDetector(DetectorOptions options, ILogger logger) : Detector<ObjectDetectionResult, ObjectDetection>(options, logger), IObjectDetector;
