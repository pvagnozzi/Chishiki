// -----------------------------------------------------------------------------
// File:        IFaceDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines a detector for faces in image frames.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Detectors.Faces;

/// <summary>Defines a detector that locates faces and optionally enriches detections with recognition metadata.</summary>
public interface IFaceDetector : IDetector<FaceDetectionResult, FaceDetection>
{
    /// <summary>Gets the strongly typed options for the face detector.</summary>
    new FaceDetectorOptions Options { get; }
}
