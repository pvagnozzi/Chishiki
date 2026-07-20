// -----------------------------------------------------------------------------
// File:        IFaceDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Interface for face detectors.
// Created:     2026-06-07
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Defines a detector that locates and optionally recognizes faces within a video frame.</summary>
public interface IFaceDetector : IDetector<FaceDetection>;
