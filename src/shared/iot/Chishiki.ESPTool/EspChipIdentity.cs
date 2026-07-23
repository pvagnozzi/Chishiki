// -----------------------------------------------------------------------------
// File:        EspChipIdentity.cs
// Author:      Piergiorgio Vagnozzi
// Description: Describes the chip identity resolved for an ESP ROM session.
// Created:     2026-06-09
// Modified:    2026-06-09
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
namespace Chishiki.ESPTool;

/// <summary>Represents the chip identity resolved for a ROM bootloader session.</summary>
public sealed class EspChipIdentity
{
    /// <summary>Gets the resolved chip target.</summary>
    public required EspChipTarget Target { get; init; }

    /// <summary>Gets the human-readable chip name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the mechanism used to resolve the chip target.</summary>
    public EspChipDetectionSource DetectionSource { get; init; }

    /// <summary>Gets the chip identifier reported by the security-info command when available.</summary>
    public uint? ChipId { get; init; }

    /// <summary>Gets the chip magic value read from the ROM detection register when available.</summary>
    public uint? MagicValue { get; init; }

    /// <summary>Gets a value indicating whether secure download mode was reported while identifying the chip.</summary>
    public bool IsSecureDownloadMode { get; init; }
}
