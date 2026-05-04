// -----------------------------------------------------------------------------
// File:        ILoggable.cs
// Author:      Piergiorgio Vagnozzi
// Description: Marker interface for components that expose a structured logger.
// Created:     2024-09-19
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace Chishiki.Core;

/// <summary>Marker interface for components that expose a structured logger.</summary>
public interface ILoggable
{
    /// <summary>Gets the logger used to record diagnostic and operational messages for the containing component.</summary>
    ILogger Logger { get; }
}
