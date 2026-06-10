// -----------------------------------------------------------------------------
// File:        EspChipDetectionSource.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines how an ESP chip target was identified for a session.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool;

/// <summary>Identifies the mechanism used to resolve the active chip target.</summary>
public enum EspChipDetectionSource
{
    /// <summary>The chip target was supplied by configuration instead of being read from the device.</summary>
    Configured = 0,

    /// <summary>The chip target was identified from the security-info chip identifier.</summary>
    ChipId = 1,

    /// <summary>The chip target was identified from security-info flags.</summary>
    SecurityInfo = 2,

    /// <summary>The chip target was identified from the ROM magic register value.</summary>
    MagicValue = 3,

    /// <summary>No concrete chip was identified and the session fell back to the generic ROM profile.</summary>
    Fallback = 4
}
