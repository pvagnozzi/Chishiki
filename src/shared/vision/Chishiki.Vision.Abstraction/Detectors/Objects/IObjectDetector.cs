// -----------------------------------------------------------------------------
// File:        IObjectDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Represents an object detector interface.
// Created:     2025-01-01
// Modified:    2026-05-31
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Objects;

/// <summary>
/// Represents an object detector interface.
/// </summary>
public interface IObjectDetector : IDetector<ObjectDetection, DetectorOptions>;
