// -----------------------------------------------------------------------------
// File:        ManagedVideoSourceOptions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Configuration options describing a video source monitor managed by the manager.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Monitors;

/// <summary>Configuration options describing a video source that should be created and managed by <see cref="IVideoSourceMonitorManager"/>.</summary>
public sealed class ManagedVideoSourceOptions
{
    /// <summary>Gets the configuration section containing the managed video sources.</summary>
    public const string SectionName = "Vision:ManagedVideoSources";

    /// <summary>Gets or sets the unique identifier assigned to the configured video source.</summary>
    public string VideoSourceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of video source to create, for example <c>Camera</c> or <c>File</c>.</summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>Gets or sets the zero-based camera index for camera-backed sources.</summary>
    public int? CameraIndex { get; set; }

    /// <summary>Gets or sets the file path for file-backed sources.</summary>
    public string? FilePath { get; set; }

    /// <summary>Gets or sets the monitor options applied to the created video source monitor.</summary>
    public VideoSourceMonitorOptions MonitorOptions { get; set; } = new();
}
