// -----------------------------------------------------------------------------
// File:        IVideoSourceMonitorFactory.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstraction for creating configured video source monitors.
// Created:     2026-06-07
// Modified:    2026-06-07
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Vision.Abstraction.Monitors;

/// <summary>Creates <see cref="IVideoSourceMonitor"/> instances from configuration.</summary>
public interface IVideoSourceMonitorFactory
{
    /// <summary>Creates a configured <see cref="IVideoSourceMonitor"/> instance.</summary>
    /// <param name="options">Configuration describing the video source monitor to create.</param>
    /// <returns>The configured <see cref="IVideoSourceMonitor"/> instance.</returns>
    IVideoSourceMonitor Create(ManagedVideoSourceOptions options);
}
